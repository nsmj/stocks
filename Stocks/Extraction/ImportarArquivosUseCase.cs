using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Stocks.Extraction.Strategies;
using Stocks.Models;

namespace Stocks.Extraction;

public class ImportarArquivosUseCase(
    [FromServices] EstrategiaImportacaoFactory estrategiaFactory,
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
        string caminhoExtracao = Path.Combine(caminhoUpload, pastaExtracao);

        if (Path.GetExtension(caminhoArquivo).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            ZipFile.ExtractToDirectory(caminhoArquivo, caminhoExtracao);
        }

        // Detecta as pastas dentro da extração e executa as estratégias correspondentes
        await ExecutarEstrategiasAsync(caminhoExtracao, estrategiaFactory);

        await calcularResultadosService.CalcularResultadosAsync();

        File.Delete(caminhoArquivo);
        Directory.Delete(caminhoExtracao, true);
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

    /// <summary>
    /// Detecta as pastas na extração e executa as estratégias apropriadas.
    /// </summary>
    private async Task ExecutarEstrategiasAsync(
        string caminhoExtracao,
        EstrategiaImportacaoFactory factory
    )
    {
        if (!Directory.Exists(caminhoExtracao))
        {
            return;
        }

        // Obtém todas as pastas dentro de ArquivosExtracao
        string[] pastas = Directory.GetDirectories(caminhoExtracao);
        List<Task> tarefasImportacao = [];

        foreach (var pastaCompleta in pastas)
        {
            string nomePasta = Path.GetFileName(pastaCompleta);

            // Verifica se existe uma estratégia para esta pasta
            var estrategia = factory.CriarEstrategia(nomePasta);

            if (estrategia is not null && Directory.Exists(pastaCompleta))
            {
                // Adiciona a tarefa de importação à lista
                tarefasImportacao.Add(estrategia.ExecutarAsync(pastaCompleta));
            }
        }

        // Executa todas as estratégias em paralelo
        if (tarefasImportacao.Count > 0)
        {
            await Task.WhenAll(tarefasImportacao);
        }
    }

    #endregion
}
