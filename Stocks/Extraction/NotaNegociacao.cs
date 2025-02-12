using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class NotaNegociacao
    {
        public List<Operacao> Operacoes { get; set; }
        public Irrf[] Irrfs { get; set; }
        public DateTime Data { get; set; }
        public decimal TotalTaxas { get; set; }
        public decimal TotalNota { get; set; }
        private Corretora _corretora { get; set; }
        private IConfiguration _configuration { get; set; }
        public decimal BaseIrrfOperacoesComuns { get; set; }
        public decimal BaseIrrfDayTrade { get; set; }

        public decimal BaseIrrfFIIs { get; set; }

        public NotaNegociacao(Corretora corretora, IConfiguration configuration)
        {
            _corretora = corretora;
            _configuration = configuration;
        }

        public async Task<(List<Operacao>, List<Irrf>)> ExtraiDadosDoArquivo(
            BancoContext db,
            string path
        )
        {
            var dadosNota = ExtractPdfData(path, _configuration["PDF_PASSWORD_1"]);

            if (dadosNota.Length == 0)
            {
                dadosNota = ExtractPdfData(path, _configuration["PDF_PASSWORD_2"]);
            }

            this.Data = _corretora.ExtrairDataNotaCorretagem(dadosNota);

            Operacoes = await _corretora.ExtrairOperacoesNotaCorretagem(db, dadosNota);
            TotalTaxas = _corretora.ExtrairTaxasNotaCorretagem(dadosNota);
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

            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
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

        private async Task<Irrf[]> ExtrairIrrf(BancoContext db, string[] dadosNota)
        {
            List<Irrf> irrfs = [];

            CalcularBaseIrrf();

            Regex regexIrrf = _corretora.ExpressaoIrrf();
            Regex regexIrrfDayTrade = _corretora.ExpressaoIrrfDayTrade();

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
                            dadosNota[i + _corretora.AjustePosicaoIrrf()]
                        );

                        if (irrfValor > 0)
                        {
                            Irrf irrf = new();

                            irrf.Data = this.Data;
                            irrf.Valor = irrfValor;

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
                        Irrf irrf = new();

                        irrf.Valor = irrfValor;
                        irrf.Data = this.Data;
                        irrf.TipoOperacao = db.TiposOperacao.Single(t => t.Nome == "Day Trade");

                        irrfs.Add(irrf);
                    }
                }
            }

            return irrfs.ToArray();
        }

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

        private void RatearTaxas()
        {
            decimal totalOperacoes = Operacoes.Sum(o => o.ValorTotal.GetValueOrDefault());

            for (int i = 0; i < Operacoes.Count(); i++)
            {
                decimal percentualOperacao =
                    Operacoes[i].ValorTotal.GetValueOrDefault() / totalOperacoes;
                Operacoes[i].Taxas = Math.Round(TotalTaxas * percentualOperacao, 2);
            }
        }
    }
}
