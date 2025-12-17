using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class Nuinvest : Corretora
    {
        public override int AjustePosicaoIrrf()
        {
            return 1;
        }

        public override Regex ExpressaoIrrf()
        {
            return new Regex(@"I.R.R.F. s/ operações. Base (.*)");
        }

        public override Regex ExpressaoIrrfDayTrade()
        {
            // TODO: Fazer algo aqui quando tiver feito algum Day Trade pela Nuinvest.
            return new Regex("fjklasjjlççlsjfjasls");
        }

        public override DateTime ExtrairDataNotaCorretagem(string[] dadosNota)
        {
            var posicaoData = Array.IndexOf(dadosNota, "Data Pregão") + 1;
            return DateTime.Parse(dadosNota[posicaoData]);
        }

        public override async Task<List<Operacao>> ExtrairOperacoesNotaCorretagem(
            BancoContext db,
            string[] dadosNota
        )
        {
            List<Operacao> operacoes = [];

            var entradasOperacoes = Corretora.FindAllIndex(dadosNota, x => x == "BOVESPA");

            foreach (var posicao in entradasOperacoes)
            {
                Operacao operacao = new();

                Match matchObj = Regex.Match(dadosNota[posicao + 3], @"(\w*) (\w{2})");

                // Compra.
                operacao.Compra = dadosNota[posicao + 1] == "C" ? 1 : 0;

                // Código ativo.
                string codigoAtivo = matchObj.Groups[1].ToString();

                // Remove the "F" at the end of the asset name, if any.
                if (codigoAtivo[codigoAtivo.Length - 1] == 'F')
                {
                    codigoAtivo = codigoAtivo[..^1];
                }

                operacao.Ativo = await db
                    .Ativos.Include("TipoAtivo")
                    .SingleAsync(a => a.Codigo == codigoAtivo);

                // Tipo Operação.
                // FIXME: Falta tratar o DAY TRADE
                if (operacao.Ativo.TipoAtivo.Nome == "FII")
                {
                    operacao.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                        t.Nome == "FII"
                    );
                }
                else
                {
                    operacao.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                        t.Nome == "Swing Trade"
                    );
                }

                // Partindo de 2 posições depois de onde tem BOVESPA (pra pular
                // o campo C ou V(compra ou venda)), vai indo sequencialmente até
                // encontrar a letra D(débito) ou C(crédito) pra daí então começar
                // a ir pra trás e achar os campos QUANTIDADE e PREÇO */
                var posicaoTemporaria = posicao + 2;

                while (dadosNota[posicaoTemporaria] != "")
                {
                    posicaoTemporaria += 1;
                }

                operacao.Quantidade = Convert.ToInt32(dadosNota[posicaoTemporaria - 4]);
                operacao.PrecoAtivo = Convert.ToDecimal(dadosNota[posicaoTemporaria - 3]);
                operacao.ValorTotal = operacao.Quantidade * operacao.PrecoAtivo;

                operacoes.Add(operacao);
            }

            return operacoes;
        }

        public override decimal ExtrairTaxasNotaCorretagem(string[] dadosNota)
        {
            int posicao = Array.IndexOf(dadosNota, "Taxa de Liquidação") + 1;
            decimal taxaLiquidacao = Convert.ToDecimal(dadosNota[posicao]);

            posicao = Array.IndexOf(dadosNota, "Taxa de Registro") + 1;
            decimal taxaRegistro = Convert.ToDecimal(dadosNota[posicao]);

            posicao = Array.IndexOf(dadosNota, "Total Bolsa") + 2;
            decimal totalBovespa = Convert.ToDecimal(dadosNota[posicao]);

            posicao = Array.IndexOf(dadosNota, "Total Corretagem/Despesas") + 2;
            decimal totalCustosOperacionais = Convert.ToDecimal(dadosNota[posicao]);

            return Math.Abs(
                Math.Round(
                    taxaLiquidacao + taxaRegistro + totalBovespa + totalCustosOperacionais,
                    2
                )
            );
        }
    }
}
