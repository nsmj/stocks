using Microsoft.AspNetCore.Mvc;
using Stocks.Interfaces;

namespace Stocks.Controllers;

/// <summary>
/// Controller responsável por gerenciar o upload e processamento de arquivos financeiros.
/// </summary>
/// <param name="logger"></param>
/// <param name="importarArquivosUseCase"></param>
public class ArquivosController(
    ILogger<ArquivosController> logger,
    IImportarArquivosUseCase importarArquivosUseCase
) : Controller
{
    /// <summary>
    /// Exibe a página de importação de arquivos.
    /// </summary>
    /// <returns></returns>
    public IActionResult Importar()
    {
        return View();
    }

    /// <summary>
    /// Processa o arquivo enviado pelo usuário.
    /// </summary>
    /// <param name="arquivo"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> ImportarAsync(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            return BadRequest("Arquivo inválido.");
        }

        try
        {
            await importarArquivosUseCase.ExecutarAsync(arquivo);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao importar o arquivo.");
            return StatusCode(500, "Erro ao processar o arquivo.");
        }
    }
}
