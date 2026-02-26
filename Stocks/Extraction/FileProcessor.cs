using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    /// <summary>
    /// Classe responsável pro processar as notas de corretagem e outros arquivos financeiros.
    /// </summary>
    public class FileProcessor
    {
        /// <summary>
        /// Processa os arquivos de notas de corretagem e outros arquivos financeiros.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="configuration"></param>
        /// <param name="posicaoFimAnoBo"></param>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task ProcessarArquivos(
            BancoContext db,
            IConfiguration configuration,
            PosicaoFimAnoBo posicaoFimAnoBo,
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

            await CalcularResultados(db);

            File.Delete(caminhoArquivo);
            Directory.Delete(Path.Combine(caminhoUpload, pastaExtracao), true);
        }

        /// <summary>
        /// Faz o upload do arquivo para o caminho especificado.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="caminhoDestino"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Importa as notas de corretagem.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="configuration"></param>
        /// <param name="caminhoArquivos"></param>
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

        /// <summary>
        /// Importa os arquivos JSON contendo operações e eventos.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="caminhoArquivos"></param>
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

        public static async Task CalcularResultados(BancoContext db)
        {
            var primeiroAnoTransacoes = await db.Operacoes.MinAsync(op => op.Data.Year);

            int anoFinal = DateTime.Now.Year;

            // Busca uma lista dos ativos que possuem operações
            var listaAtivos = await db.Operacoes.Select(op => op.AtivoId).Distinct().ToListAsync();

            for (int anoAtual = primeiroAnoTransacoes; anoAtual <= anoFinal; anoAtual++)
            {
                foreach (var ativoId in listaAtivos)
                {
                    CalcularResultadosAtivoAno(db, anoAtual, ativoId);
                }
            }
        }

        /// <summary>
        /// Calcula os resultados das operações e eventos, atualizando o lucro líquido das operações.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ano"></param>
        /// <param name="ativoId"></param>
        public static async void CalcularResultadosAtivoAno(BancoContext db, int ano, int ativoId)
        {
            var anoInicio = $"{ano}-01-01";
            var anoFim = $"{ano}-12-31";

            var eventosOperacoes = db.Database.SqlQuery<DadosCalculoResultadoDTO>(
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
                        WHERE o.ativo_id = {ativoId} AND o.data BETWEEN {anoInicio} AND {anoFim}
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
                        WHERE e.ativo_id = {ativoId} AND e.data BETWEEN {anoInicio} AND {anoFim}
                        ORDER BY AtivoId, Data
                    "
            );

            var ultimaPosicaoFimAno = await db
                .PosicoesFimAno.Where(p => p.AtivoId == ativoId && p.Ano == ano - 1)
                .FirstOrDefaultAsync();

            decimal precoMedio = ultimaPosicaoFimAno != null ? ultimaPosicaoFimAno.PrecoMedio : 0;
            decimal ultimoPMCompra = precoMedio;
            int posicaoAnterior = ultimaPosicaoFimAno != null ? ultimaPosicaoFimAno.Posicao : 0;
            int posicaoAtual = 0;

            decimal precoMedioVenda;
            decimal lucroLiquido;
            Dictionary<string, PosicaoFimAno> posicoesFimAno = [];

            await foreach (var eventoOperacao in eventosOperacoes.AsAsyncEnumerable())
            {
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
            }

            if (eventosOperacoes.Any())
            {
                // Só insere uma posição no final do ano caso não tenha zerado a posição durante o ano
                if (posicaoAtual > 0)
                {
                    PosicaoFimAno posicaoFimAno = new()
                    {
                        AtivoId = ativoId,
                        Ano = ano,
                        Posicao = posicaoAtual,
                        PrecoMedio = ultimoPMCompra,
                        CustoTotal = ultimoPMCompra * posicaoAtual,
                    };

                    db.PosicoesFimAno.Add(posicaoFimAno);
                }

                // Atualiza a posição e preço médio do ativo
                var ativo = db.Ativos.Find(ativoId);

                if (ativo != null)
                {
                    ativo.Posicao = posicaoAtual;
                    ativo.PrecoMedio = ultimoPMCompra;

                    db.Ativos.Update(ativo);
                }
            }
            else
            {
                // Se não houver operações ou eventos no ano, mantém a posição do ano anterior (se houver)
                if (ultimaPosicaoFimAno != null && ultimaPosicaoFimAno.Posicao > 0)
                {
                    PosicaoFimAno novaPosicao = new()
                    {
                        AtivoId = ativoId,
                        Ano = ano,
                        Posicao = ultimaPosicaoFimAno.Posicao,
                        PrecoMedio = ultimaPosicaoFimAno.PrecoMedio,
                        CustoTotal = ultimaPosicaoFimAno.CustoTotal,
                    };

                    db.PosicoesFimAno.Add(novaPosicao);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
