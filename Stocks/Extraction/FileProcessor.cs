using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class FileProcessor
    {
        /* public static async Task ProcessFiles()
        {
            DbConnection.Reset();

            var chObjects = Channel.CreateUnbounded<IStorable[]>();

            _ = Task.Run(() => ImportFiles(chObjects));

            //await SaveData(chObjects);
        }
 */
        public async void ImportarNotasCorretagem(BancoContext db, IConfiguration configuration)
        {
            string[] files = Directory.GetFiles(
                Path.Join("Arquivos", "NotasCorretagem"),
                "*",
                SearchOption.AllDirectories
            );

            List<Operacao> operacoes = [];
            List<Irrf> irrfs = [];

            foreach (var file in files)
            {
                Regex rgx = new(
                    Path.Combine("Arquivos", "NotasCorretagem", "(.*)", "(.*)")
                        .Replace("\\", "\\\\")
                );
                Match matchObj = rgx.Match(file);
                var strategy = matchObj.Groups[1].ToString();

                var corretora = Corretora.Factory(strategy);

                NotaNegociacao notaNegociacao = new(corretora, configuration);

                var (operacoesInserir, irrfsInserir) = await notaNegociacao.ExtraiDadosDoArquivo(
                    db,
                    file
                );

                await db.Operacoes.AddRangeAsync(operacoesInserir);

                await db.Irrfs.AddRangeAsync(irrfsInserir);
            }

            await db.SaveChangesAsync();
        }

        public async void ImportarArquivosJson(BancoContext db)
        {
            string[] files = Directory.GetFiles(
                Path.Join("Arquivos", "Json"),
                "*",
                SearchOption.AllDirectories
            );

            foreach (var file in files)
            {
                JsonInformation jsonInformation = new();

                var (operacoes, eventos) = jsonInformation.ExtrairDadosArquivo(db, file);

                await db.Operacoes.AddRangeAsync(operacoes);

                await db.Eventos.AddRangeAsync(eventos);
            }

            await db.SaveChangesAsync();
        }

        /*private static async Task SaveData(Channel<IStorable[]> chObjects)
        {
            using SqliteConnection conn = DbConnection.Get();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            await foreach (IStorable[] results in chObjects.Reader.ReadAllAsync())
            {
                foreach (IStorable o in results)
                {
                    if (o is not null)
                    {
                        SqliteCommand cmd = conn.CreateCommand();

                        o.PrepareSqliteCommand(ref cmd);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            transaction.Commit();
        }*/

        /* private static void ImportFiles(Channel<IStorable[]> chObjects)
        {
            string[] files = Directory.GetFiles("Arquivos", "*", SearchOption.AllDirectories);
            var tasks = new List<Task>();

            foreach (var file in files)
            {
                // FIXME: Melhorar isso.
                if (file == "Arquivos\\.gitkeep")
                {
                    continue;
                }

                var t = Task.Run(() =>
                {
                    Regex rgx = new(@"Arquivos\\(.*)\\(.*)");

                    Match matchObj = rgx.Match(file);
                    var strategy = matchObj.Groups[1].ToString();

                    var extractor = IExtractor.Factory(strategy);

                    if (extractor is null)
                    {
                        return;
                    }

                    var results = extractor.ExtractFileData(file);

                    chObjects.Writer.WriteAsync(results);

                });

                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());

            chObjects.Writer.Complete();
        } */

        public static async void CalcularResultados(BancoContext db)
        {
            var eventosOperacoes = db.Database.SqlQuery<DadosCalculoResultado>(
                @$"
                        SELECT
                            o.id AS Id,
                            'Operacao' AS Tipo,
                            data AS Data,
                            null AS Fator,
                            quantidade AS Quantidade,
                            preco_ativo AS PrecoAtivo,
                            valor_total AS ValorTotal,
                            compra AS Compra,
                            taxas AS Taxas,
                            a.id AS AtivoId,
                            top.nome AS 'TipoOperacaoEvento'
                        FROM operacao o
                        LEFT JOIN ativo a ON o.ativo_id = a.id
                        LEFT JOIN tipo_operacao top ON o.tipo_operacao_id = top.id
                        UNION ALL
                        SELECT
                            e.id AS Id,
                            'Evento' AS Tipo,
                            data AS Data,
                            fator AS Fator,
                            null AS Quantidade,
                            Valor AS PrecoAtivo,
                            null AS ValorTotal,
                            null AS Compra,
                            null AS Taxas,
                            a.id AS AtivoId,
                            te.nome
                        FROM evento e
                        LEFT JOIN ativo a ON e.ativo_id = a.id
                        LEFT JOIN tipo_evento te ON e.tipo_evento_id = te.id
                        ORDER BY AtivoId, Data
                    "
            );

            long ativoAnterior = 0;
            decimal ultimoPMCompra = 0M;
            int posicaoAnterior = 0;
            int posicaoAtual = 0;
            decimal precoMedio = 0M;
            decimal precoMedioVenda = 0M;
            decimal lucroLiquido = 0M;

            foreach (var eventoOperacao in eventosOperacoes)
            {
                if (eventoOperacao.AtivoId != ativoAnterior)
                {
                    ultimoPMCompra = 0;
                    posicaoAnterior = 0;
                    posicaoAtual = 0;
                    ativoAnterior = eventoOperacao.AtivoId;
                }

                if (eventoOperacao.Tipo == "Operacao")
                {
                    //var taxas = eventoOperacao.Taxas == null ? 0.0 : eventoOperacao.Taxas;
                    var taxas = eventoOperacao.Taxas.GetValueOrDefault();

                    if (eventoOperacao.Compra == 1)
                    {
                        posicaoAtual =
                            posicaoAnterior + eventoOperacao.Quantidade.GetValueOrDefault();
                        precoMedio =
                            (
                                (ultimoPMCompra * posicaoAnterior)
                                + Convert.ToDecimal(eventoOperacao.ValorTotal.GetValueOrDefault())
                                + Convert.ToDecimal(taxas)
                            ) / posicaoAtual;
                        ultimoPMCompra = precoMedio;
                    }
                    else
                    {
                        posicaoAtual =
                            posicaoAnterior - eventoOperacao.Quantidade.GetValueOrDefault();
                        precoMedioVenda =
                            (
                                Convert.ToDecimal(eventoOperacao.ValorTotal.GetValueOrDefault())
                                - Convert.ToDecimal(taxas)
                            ) / eventoOperacao.Quantidade.GetValueOrDefault();
                        lucroLiquido = Math.Round(
                            (precoMedioVenda - ultimoPMCompra)
                                * eventoOperacao.Quantidade.GetValueOrDefault(),
                            2
                        );

                        var operacao = db.Operacoes.Find(eventoOperacao.Id);

                        if (operacao != null)
                        {
                            // Atualiza o lucro líquido.
                            operacao.LucroLiquido = lucroLiquido;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                else if (eventoOperacao.Tipo == "Evento")
                {
                    if (eventoOperacao.TipoOperacaoEvento == "Grupamento")
                    {
                        posicaoAtual /= eventoOperacao.Fator.GetValueOrDefault();
                        precoMedio = ultimoPMCompra * eventoOperacao.Fator.GetValueOrDefault();
                    }
                    else if (eventoOperacao.TipoOperacaoEvento == "Desdobramento")
                    {
                        posicaoAtual *= eventoOperacao.Fator.GetValueOrDefault();
                        precoMedio = ultimoPMCompra / eventoOperacao.Fator.GetValueOrDefault();
                    }
                    else if (eventoOperacao.TipoOperacaoEvento == "Bonificação")
                    {
                        var acoesBonificadas = (posicaoAnterior * eventoOperacao.Fator) / 100;
                        posicaoAtual += acoesBonificadas.GetValueOrDefault();
                        precoMedio =
                            (
                                (ultimoPMCompra * posicaoAnterior)
                                + (
                                    eventoOperacao.PrecoAtivo.GetValueOrDefault()
                                    * acoesBonificadas.GetValueOrDefault()
                                )
                            ) / posicaoAtual;
                    }
                }

                posicaoAnterior = posicaoAtual;

                ultimoPMCompra = precoMedio;

                var ativo = db.Ativos.Find(eventoOperacao.AtivoId);

                if (ativo != null)
                {
                    ativo.Posicao = posicaoAtual;
                    ativo.PrecoMedio = ultimoPMCompra;
                }

                await db.SaveChangesAsync();
            }

            //transaction.Commit();
        }
    }

    /*public interface IExtractor
    {
        IStorable[] ExtractFileData(string path);

        static IExtractor Factory(string nome)
        {
            switch (nome)
            {
                case "Nuinvest":
                    return new NotaNegociacao() { Corretora = new Nuinvest() };
                case "Clear":
                    return new NotaNegociacao() { Corretora = new Clear() };
                case "JsonInformation":
                    return new JsonInformation();
                case "B3Report":
                    return new B3Report();
                default:
                    throw new Exception("Strategy not defined");
            }
        }
    }*/

    /// <summary>
    /// Objetos que podem ser inseridos no banco de dados.
    /// </summary>
    public interface IStorable
    {
        public int Id { get; set; }
    }
}
