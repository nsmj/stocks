using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    /// <summary>
    /// Classe responsável por extrair dados de notas de negociação.
    /// </summary>
    /// <param name="corretora"></param>
    /// <param name="configuration"></param>
    public class NotaNegociacao(Corretora corretora, IConfiguration configuration)
    {
        public List<Operacao> Operacoes { get; set; }
        public Irrf[] Irrfs { get; set; }
        public DateTime Data { get; set; }
        public decimal TotalTaxas { get; set; }
        public decimal TotalNota { get; set; }

        public decimal BaseIrrfOperacoesComuns { get; set; }
        public decimal BaseIrrfDayTrade { get; set; }

        public decimal BaseIrrfFIIs { get; set; }

        /// <summary>
        /// Extrai os dados do arquivo PDF da nota de negociação e retorna as operações e IRRFs.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<(List<Operacao>, List<Irrf>)> ExtraiDadosDoArquivo(
            BancoContext db,
            string path
        )
        {
            var dadosNota = ExtractPdfData(path, configuration["PDF_PASSWORD_1"]);

            if (dadosNota.Length == 0)
            {
                dadosNota = ExtractPdfData(path, configuration["PDF_PASSWORD_2"]);
            }

            this.Data = corretora.ExtrairDataNotaCorretagem(dadosNota);

            Operacoes = await corretora.ExtrairOperacoesNotaCorretagem(db, dadosNota);
            TotalTaxas = corretora.ExtrairTaxasNotaCorretagem(dadosNota);
            RatearTaxas();

            Irrfs = await ExtrairIrrf(db, dadosNota);

            List<Operacao> operacoes = [];
            List<Irrf> irrfs = [];

            // Seta as datas das operações de acordo com a data da nota de corretagem.
            if (Operacoes is not null)
            {
                foreach (Operacao operacao in Operacoes)
                {
                    operacao.Data = Data;
                    operacoes.Add(operacao);
                }
            }

            if (Irrfs is not null)
            {
                foreach (var irrf in Irrfs)
                {
                    irrfs.Add(irrf);
                }
            }

            return (operacoes, irrfs);
        }

        /// <summary>
        /// Extrai os dados do PDF da nota de negociação.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="senhaArquivo"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="PlatformNotSupportedException"></exception>
        private string[] ExtractPdfData(string path, string senhaArquivo)
        {
            string mutoolPath;
            string arguments;
            string fileName = Path.GetFileName(path).Replace(".pdf", ".txt");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                mutoolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mutool.exe");
                if (!File.Exists(mutoolPath))
                {
                    throw new FileNotFoundException("O arquivo mutool.exe não foi encontrado.");
                }
                arguments = $"convert -p {senhaArquivo} -F text -o {fileName} {path}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                mutoolPath = "/usr/bin/mutool";
                if (!File.Exists(mutoolPath))
                {
                    throw new FileNotFoundException("O arquivo mutool não foi encontrado.");
                }
                arguments = $"-c \"mutool convert -p {senhaArquivo} -F text -o {fileName} {path}\"";
            }
            else
            {
                throw new PlatformNotSupportedException("Sistema operacional não suportado.");
            }

            using (System.Diagnostics.Process pProcess = new())
            {
                pProcess.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? mutoolPath
                    : "/bin/bash";
                pProcess.StartInfo.Arguments = arguments;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.Start();
                pProcess.WaitForExit();
            }

            var dadosNota = File.ReadAllLines(fileName);
            File.Delete(fileName);

            return dadosNota;
        }

        /// <summary>
        /// Extrai os dados de IRRF da nota de negociação.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dadosNota"></param>
        /// <returns></returns>
        private async Task<Irrf[]> ExtrairIrrf(BancoContext db, string[] dadosNota)
        {
            List<Irrf> irrfs = [];

            CalcularBaseIrrf();

            Regex regexIrrf = corretora.ExpressaoIrrf();
            Regex regexIrrfDayTrade = corretora.ExpressaoIrrfDayTrade();

            for (int i = 0; i < dadosNota.Length; i++)
            {
                // Operações comuns e FIIs.
                Match matchIrrf = regexIrrf.Match(dadosNota[i]);

                if (matchIrrf.Success)
                {
                    decimal valorBase = Convert.ToDecimal(matchIrrf.Groups[1].ToString());

                    if (valorBase > 0)
                    {
                        decimal irrfValor = Convert.ToDecimal(
                            dadosNota[i + corretora.AjustePosicaoIrrf()]
                        );

                        if (irrfValor > 0)
                        {
                            Irrf irrf = new() { Data = Data, Valor = irrfValor };

                            if (valorBase == BaseIrrfOperacoesComuns)
                            {
                                irrf.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                                    t.Nome == "Swing Trade"
                                );
                            }
                            else if (valorBase == BaseIrrfFIIs)
                            {
                                irrf.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                                    t.Nome == "FII"
                                );
                            }
                            else
                            {
                                continue;
                            }

                            irrfs.Add(irrf);
                        }
                    }
                }

                // Day Trade.
                Match matchIrrfDayTrade = regexIrrfDayTrade.Match(dadosNota[i]);

                if (matchIrrfDayTrade.Success)
                {
                    decimal irrfValor = Convert.ToDecimal(matchIrrfDayTrade.Groups[3].ToString());

                    if (irrfValor > 0)
                    {
                        Irrf irrf = new()
                        {
                            Valor = irrfValor,
                            Data = this.Data,
                            TipoOperacao = db.TiposOperacao.Single(t => t.Nome == "Day Trade"),
                        };

                        irrfs.Add(irrf);
                    }
                }
            }

            return [.. irrfs];
        }

        /// <summary>
        /// Calcula a base de IRRF para operações comuns, day trade e FIIs.
        /// </summary>
        private void CalcularBaseIrrf()
        {
            if (Operacoes is not null)
            {
                foreach (Operacao operacao in Operacoes)
                {
                    if (operacao.Compra == 0)
                    {
                        switch (operacao.TipoOperacao.Nome)
                        {
                            case "Swing Trade":
                                BaseIrrfOperacoesComuns += operacao.ValorTotal.GetValueOrDefault();
                                break;
                            case "FII":
                                BaseIrrfFIIs += operacao.ValorTotal.GetValueOrDefault();
                                break;
                        }
                    }
                }

                BaseIrrfOperacoesComuns = Math.Round(BaseIrrfOperacoesComuns, 2);
                BaseIrrfFIIs = Math.Round(BaseIrrfFIIs, 2);
            }
        }

        /// <summary>
        /// Rateia as taxas entre as operações.
        /// </summary>
        private void RatearTaxas()
        {
            decimal totalOperacoes = Operacoes.Sum(o => o.ValorTotal.GetValueOrDefault());

            for (int i = 0; i < Operacoes.Count; i++)
            {
                decimal percentualOperacao =
                    Operacoes[i].ValorTotal.GetValueOrDefault() / totalOperacoes;
                Operacoes[i].Taxas = Math.Round(TotalTaxas * percentualOperacao, 2);
            }
        }
    }
}
