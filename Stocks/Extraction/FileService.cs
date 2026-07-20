using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Bo;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Models;

namespace Stocks.Extraction
{
    public class FileService(
        [FromServices] BancoContext db,
        [FromServices] IConfiguration configuration,
        [FromServices] PdfExtractor pdfExtractor
    )
    {
        /// <summary>
        /// Importa as notas de corretagem.
        /// </summary>
        /// <param name="caminhoArquivos"></param>
        public async Task ImportarNotasCorretagemAsync(string caminhoArquivos)
        {
            string[] files = Directory.GetFiles(caminhoArquivos, "*", SearchOption.AllDirectories);
            List<Task<DadosNotaNegociacaoDto>> retornoExtracaoDados = [];

            foreach (var file in files)
            {
                Regex rgx = new(
                    Path.Combine(caminhoArquivos, "(.*)", "(.*)").Replace("\\", "\\\\")
                );
                Match matchObj = rgx.Match(file);
                var strategy = matchObj.Groups[1].ToString();

                var corretora = Corretora.Factory(strategy);

                NotaNegociacao notaNegociacao = new(corretora, configuration, db, pdfExtractor);

                retornoExtracaoDados.Add(notaNegociacao.ExtraiDadosDoArquivoAsync(file));
            }

            var dados = await Task.WhenAll(retornoExtracaoDados);

            foreach (var dadosNota in dados)
            {
                await db.Operacoes.AddRangeAsync(dadosNota.Operacoes);
                await db.Irrfs.AddRangeAsync(dadosNota.Irrfs);
            }

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Importa os arquivos JSON contendo operações e eventos.
        /// </summary>
        /// <param name="caminhoArquivos"></param>
        public async Task ImportarArquivosJsonAsync(string caminhoArquivos)
        {
            string[] files = Directory.GetFiles(caminhoArquivos, "*", SearchOption.AllDirectories);
            List<Task<DadosArquivoJsonDto>> retornoExtracaoDados = [];

            foreach (var file in files)
            {
                JsonInformation jsonInformation = new(db);

                retornoExtracaoDados.Add(jsonInformation.ExtrairDadosArquivoAsync(file));
            }

            var dados = await Task.WhenAll(retornoExtracaoDados);

            foreach (var dadosArquivoJson in dados)
            {
                await db.Operacoes.AddRangeAsync(dadosArquivoJson.Operacoes);
                await db.Eventos.AddRangeAsync(dadosArquivoJson.Eventos);
            }

            await db.SaveChangesAsync();
        }
    }
}
