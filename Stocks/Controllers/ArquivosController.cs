using Microsoft.AspNetCore.Mvc;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Controllers;

/// <summary>
/// Controller responsável por gerenciar o upload e processamento de arquivos financeiros.
/// </summary>
/// <param name="logger"></param>
/// <param name="db"></param>
/// <param name="configuration"></param>
/// <param name="fileProcessor"></param>
public class ArquivosController(
    ILogger<ArquivosController> logger,
    BancoContext db,
    IConfiguration configuration,
    FileProcessor fileProcessor
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
    public async Task<IActionResult> Importar(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            return BadRequest("Arquivo inválido.");
        }

        try
        {
            await fileProcessor.ProcessarArquivos(db, configuration, arquivo);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao importar o arquivo.");
            return StatusCode(500, "Erro ao processar o arquivo.");
        }
    }
}
