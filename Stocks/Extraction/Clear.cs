using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class Clear : Corretora
    {
        public override int AjustePosicaoIrrf()
        {
            return -1;
        }

        public override Regex ExpressaoIrrf()
        {
            return new Regex(@"I.R.R.F. s/ operações, base R\$(.*)");
        }

        public override Regex ExpressaoIrrfDayTrade()
        {
            return new Regex(@"IRRF Day Trade: Base R\$ (.*),(.*) Projeção R\$ (.*)");
        }

        public override DateTime ExtrairDataNotaCorretagem(string[] dadosNota)
        {
            var posicaoData = Array.IndexOf(dadosNota, "Data pregão") + 2;
            return DateTime.Parse(dadosNota[posicaoData]);
        }

        public override async Task<List<Operacao>> ExtrairOperacoesNotaCorretagem(
            BancoContext db,
            string[] dadosNota
        )
        {
            List<Operacao> operacoes = [];

            var entradasOperacoes = FindAllIndex(dadosNota, x => x == "1-BOVESPA");

            if (!entradasOperacoes.Any())
            {
                entradasOperacoes = FindAllIndex(dadosNota, x => x == "B3 RV LISTADO");
            }

            foreach (var posicao in entradasOperacoes)
            {
                Operacao operacao = new();

                // O que está em
                // dadosNota[posicao + 1]
                // deveria ser "C" ou "V". Entretanto, às vezes vem algo como "C FRACIONARIO", ou seja,
                // junta com a informação do campo que deveria vir imediatamente após.
                // Por isso, é feita a verificação abaixo para contornar o problema quando acontece.

                string compra;
                int posicaoNomeAtivo;
                string tipoMercado;

                if (dadosNota[posicao + 1].Length > 1)
                {
                    posicaoNomeAtivo = posicao + 2;
                    tipoMercado = dadosNota[posicao + 1][2..];

                    // Pega o primeiro caractere ("C" ou "V")
                    compra = dadosNota[posicao + 1][..1];
                }
                else
                {
                    posicaoNomeAtivo = posicao + 3;
                    tipoMercado = dadosNota[posicao + 2];
                    compra = dadosNota[posicao + 1];
                }

                Match matchObj = Regex.Match(
                    dadosNota[posicaoNomeAtivo],
                    @"(.*[^\s{11}])\s{10}(\w{2,6})"
                );

                var codigo = matchObj.Groups[1].ToString();

                string tipoAcao = matchObj.Groups[2].ToString();

                // Se for venda de direito de subscrição, nem contabilizo. Dá muito rolo no IR.
                if (tipoAcao == "DO")
                {
                    continue;
                }

                if (tipoMercado == "OPCAO DE COMPRA" || tipoMercado == "OPCAO DE VENDA")
                {
                    var conversao = new Dictionary<string, string>()
                    {
                        { "OPCAO DE COMPRA", "CALL" },
                        { "OPCAO DE VENDA", "PUT" },
                    };

                    var tipoAtivo = await db.TiposAtivo.SingleAsync(t =>
                        t.Nome == conversao[tipoMercado]
                    );

                    var match = Regex.Match(dadosNota[posicaoNomeAtivo], @"\d{2}/\d{2} \w{5}\d{3}");

                    codigo = match.Groups[0].ToString();

                    var ativo = await db
                        .Ativos.Where(a => a.Codigo == codigo)
                        .FirstOrDefaultAsync();

                    if (ativo == null)
                    {
                        await db.Ativos.AddAsync(
                            new Ativo()
                            {
                                Codigo = codigo,
                                Nome = codigo,
                                DescricaoNota = codigo,
                                TipoAtivo = tipoAtivo,
                                TipoAcao = tipoAcao,
                                Cnpj = "",
                            }
                        );

                        await db.SaveChangesAsync();
                    }
                }

                // Compra.
                operacao.Compra = compra == "C" ? 1 : 0;

                var ativos = db.Ativos.Include("TipoAtivo").Where(a => a.DescricaoNota == codigo);

                // Alguns ativos têm o mesmo valor no campo DescricaoNota, por isso nesses casos
                // é necessário filtrar também pelo tipo_acao. Não dá pra filtrar direto pelo
                // tipo_acao porque às vezes o arquivo PDF traz um valor em branco no que deveria
                // ser o tipo_acao.
                if (ativos.Count() > 1)
                {
                    ativos = ativos.Where(a => a.TipoAcao == matchObj.Groups[2].ToString());
                }

                // Ativo.
                operacao.Ativo = await ativos.FirstAsync();

                /* Partindo de 2 posições depois de onde tem 1-BOVESPA (pra pular
                o campo C ou V (compra ou venda)), vai indo sequencialmente até
                encontrar a letra D (débito) ou C (crédito) pra daí então começar
                a ir pra trás e achar os campos QUANTIDADE e PREÇO

                Como os campos depois do nome do ativo são incertos (às vezes tem observação, às vezes não),
                o ponto mais "seguro" pra achar os outros campos é o "1-BOVESPA" do próximo ativo ou, se este
                não existir (por já estarmos no último ativo), usamos o "NOTA DE NEGOCIAÇÃO".
                */

                int posicaoTemporaria = posicao + 1;

                var temp = new string[4]
                {
                    "1-BOVESPA",
                    "B3 RV LISTADO",
                    "NOTA DE NEGOCIAÇÃO",
                    "NOTA DE CORRETAGEM",
                };

                while (!((IList<string>)temp).Contains(dadosNota[posicaoTemporaria]))
                {
                    posicaoTemporaria += 1;
                }

                // Caso haja observações — no momento usando apenas para indicar quando for Day Trade.
                string observacoes = "";
                if (posicaoTemporaria - 7 == posicaoNomeAtivo)
                {
                    int posicaoObservacoes = posicaoTemporaria - 6;
                    observacoes = dadosNota[posicaoObservacoes].Replace("#", "").Replace("2", "");
                }

                if (observacoes == "D")
                {
                    operacao.TipoOperacao = db.TiposOperacao.Single(t => t.Nome == "Day Trade");
                }

                operacao.Quantidade = (int)Convert.ToDouble(dadosNota[posicaoTemporaria - 5]);
                operacao.PrecoAtivo = Convert.ToDecimal(dadosNota[posicaoTemporaria - 4]);
                operacao.ValorTotal = Math.Round(
                    operacao.Quantidade.GetValueOrDefault()
                        * operacao.PrecoAtivo.GetValueOrDefault(),
                    2
                );

                if (operacao.Ativo.TipoAtivo.Nome == "FII")
                {
                    operacao.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                        t.Nome == "FII"
                    );
                }
                else if (operacao.TipoOperacao?.Nome != "Day Trade")
                {
                    // Caso já não tenha sido marcado como Day Trade mais acima pelas observações.
                    operacao.TipoOperacao = await db.TiposOperacao.SingleAsync(t =>
                        t.Nome == "Swing Trade"
                    );
                }

                operacoes.Add(operacao);
            }

            //db.SaveChanges();

            return operacoes;
        }

        public override decimal ExtrairTaxasNotaCorretagem(string[] dadosNota)
        {
            int posicao = Array.IndexOf(dadosNota, "Taxa de liquidação") - 1;
            decimal taxaLiquidacao = Convert.ToDecimal(dadosNota[posicao]);

            posicao = Array.IndexOf(dadosNota, "Taxa de Registro") - 1;
            decimal taxaRegistro = Convert.ToDecimal(dadosNota[posicao]);

            posicao = Array.IndexOf(dadosNota, "Total Bovespa / Soma") - 1;
            decimal totalBovespa = Convert.ToDecimal(dadosNota[posicao]);

            /*
            Total de custos operacionais
            Até dia 23/12/2019 escrevia-se "Total corretagem" ao invés de
            "Total Custos"	*/

            var dataNota = ExtrairDataNotaCorretagem(dadosNota);
            var data = DateTime.Parse("23/12/2019");

            if (dataNota > data)
            {
                posicao = Array.IndexOf(dadosNota, "Total Custos / Despesas") - 1;
            }
            else
            {
                posicao = Array.IndexOf(dadosNota, "Total corretagem / Despesas") - 1;
            }

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
