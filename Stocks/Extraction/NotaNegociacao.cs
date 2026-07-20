using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Models;

namespace Stocks.Extraction
{
    /// <summary>
    /// Classe responsável por extrair dados de notas de negociação.
    /// </summary>
    /// <param name="corretora"></param>
    /// <param name="configuration"></param>
    /// <param name="db"></param>
    public class NotaNegociacao(
        Corretora corretora,
        IConfiguration configuration,
        BancoContext db,
        PdfExtractor pdfExtractor
    )
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
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<DadosNotaNegociacaoDto> ExtraiDadosDoArquivoAsync(string path)
        {
            var dadosNota = pdfExtractor.ExtrairDadosPdf(path, configuration["PDF_PASSWORD_1"]);

            if (dadosNota.Length == 0)
            {
                dadosNota = pdfExtractor.ExtrairDadosPdf(path, configuration["PDF_PASSWORD_2"]);
            }

            this.Data = corretora.ExtrairDataNotaCorretagem(dadosNota);

            Operacoes = await corretora.ExtrairOperacoesNotaCorretagem(db, dadosNota);
            TotalTaxas = corretora.ExtrairTaxasNotaCorretagem(dadosNota);
            RatearTaxas();

            Irrfs = await ExtrairIrrf(dadosNota);

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

            return new DadosNotaNegociacaoDto(operacoes, irrfs);
        }

        #region Métodos privados.

        /// <summary>
        /// Extrai os dados de IRRF da nota de negociação.
        /// </summary>
        /// <param name="dadosNota"></param>
        /// <returns></returns>
        private async Task<Irrf[]> ExtrairIrrf(string[] dadosNota)
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
        #endregion
    }
}
