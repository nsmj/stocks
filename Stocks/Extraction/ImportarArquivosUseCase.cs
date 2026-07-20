using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Stocks.Models;

namespace Stocks.Extraction;

public class ImportarArquivosUseCase(
    [FromServices] PdfExtractor pdfExtractor,
    [FromServices] FileService fileService,
    [FromServices] CalcularResultadosService calcularResultadosService,
    [FromServices] IConfiguration configuration
)
{
    public async Task ExecuteAsync(IFormFile arquivo)
    {
        DbConnection.Reset();

        string caminhoUpload =
            configuration["CaminhoUpload"]
            ?? throw new ArgumentNullException(
                "CaminhoUpload",
                "Configuration value for 'CaminhoUpload' cannot be null."
            );

        string caminhoArquivo = await UploadArquivoAsync(arquivo, caminhoUpload);
        string pastaExtracao = "ArquivosExtracao";

        if (Path.GetExtension(caminhoArquivo).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            ZipFile.ExtractToDirectory(caminhoArquivo, Path.Combine(caminhoUpload, pastaExtracao));
        }

        var importarNotasCorretagemTask = fileService.ImportarNotasCorretagemAsync(
            Path.Combine(caminhoUpload, pastaExtracao, "NotasCorretagem")
        );

        var importarArquivosJsonTask = fileService.ImportarArquivosJsonAsync(
            Path.Combine(caminhoUpload, pastaExtracao, "Json")
        );

        await Task.WhenAll(importarNotasCorretagemTask, importarArquivosJsonTask);

        await calcularResultadosService.CalcularResultadosAsync();

        File.Delete(caminhoArquivo);
        Directory.Delete(Path.Combine(caminhoUpload, pastaExtracao), true);
    }

    #region Métodos privados.
    /// <summary>
    /// Faz o upload do arquivo para o caminho especificado.
    /// </summary>
    /// <param name="arquivo"></param>
    /// <param name="caminhoDestino"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<string> UploadArquivoAsync(IFormFile arquivo, string caminhoDestino)
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
    #endregion
}
