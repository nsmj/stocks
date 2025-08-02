using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class FileProcessor
    {
        public async Task ProcessarArquivos(
            BancoContext db,
            IConfiguration configuration,
            IFormFile arquivo
        )
        {
            DbConnection.Reset();

            string caminhoUpload =
                configuration["CaminhoUpload"]
                ?? throw new ArgumentNullException(
                    "CaminhoUpload",
                    "Configuration value for 'CaminhoUpload' cannot be null."
                );

            string caminhoArquivo = await UploadArquivo(arquivo, caminhoUpload);
            string pastaExtracao = "ArquivosExtracao";

            if (
                Path.GetExtension(caminhoArquivo).Equals(".zip", StringComparison.OrdinalIgnoreCase)
            )
            {
                ZipFile.ExtractToDirectory(
                    caminhoArquivo,
                    Path.Combine(caminhoUpload, pastaExtracao)
                );
            }

            ImportarNotasCorretagem(
                db,
                configuration,
                Path.Combine(caminhoUpload, pastaExtracao, "NotasCorretagem")
            );

            ImportarArquivosJson(db, Path.Combine(caminhoUpload, pastaExtracao, "Json"));

            CalcularResultados(db);

            File.Delete(caminhoArquivo);
            Directory.Delete(Path.Combine(caminhoUpload, pastaExtracao), true);
        }

        public async Task<string> UploadArquivo(IFormFile arquivo, string caminhoDestino)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                throw new ArgumentException("Arquivo inválido.");
            }

            // FIXME: Mudar o arquivo.FileName para outra coisa.
            string caminhoArquivo = Path.Combine(caminhoDestino, arquivo.FileName);

            using var stream = new FileStream(caminhoArquivo, FileMode.Create);
            await arquivo.CopyToAsync(stream);

            return caminhoArquivo;
        }

        public async void ImportarNotasCorretagem(
            BancoContext db,
            IConfiguration configuration,
            string caminhoArquivos
        )
        {
            string[] files = Directory.GetFiles(caminhoArquivos, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                Regex rgx = new(
                    Path.Combine(caminhoArquivos, "(.*)", "(.*)").Replace("\\", "\\\\")
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

        public async void ImportarArquivosJson(BancoContext db, string caminhoArquivos)
        {
            string[] files = Directory.GetFiles(caminhoArquivos, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                JsonInformation jsonInformation = new();

                var (operacoes, eventos) = jsonInformation.ExtrairDadosArquivo(db, file);

                await db.Operacoes.AddRangeAsync(operacoes);

                await db.Eventos.AddRangeAsync(eventos);
            }

            await db.SaveChangesAsync();
        }

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
            decimal precoMedioVenda;
            decimal lucroLiquido;
            Dictionary<string, PosicaoFimAno> posicoesFimAno = [];

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

                var PosicaoFimAno = new PosicaoFimAno()
                {
                    Ano = Convert.ToInt32(eventoOperacao.Data?[0..4]),
                    PrecoMedio = ultimoPMCompra,
                    Posicao = posicaoAtual,
                    CustoTotal = ultimoPMCompra * posicaoAtual,
                    AtivoId = eventoOperacao.AtivoId,
                };

                var hashAnoAtivo = $"{PosicaoFimAno.Ano}_{eventoOperacao.AtivoId}";
                posicoesFimAno[hashAnoAtivo] = PosicaoFimAno;

                var ativo = db.Ativos.Find(eventoOperacao.AtivoId);

                if (ativo != null)
                {
                    ativo.Posicao = posicaoAtual;
                    ativo.PrecoMedio = ultimoPMCompra;

                    db.Ativos.Update(ativo);
                }
            }

            foreach (var posicao in posicoesFimAno.Values)
            {
                if (posicao.Posicao > 0)
                {
                    db.PosicoesFimAno.Add(posicao);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
