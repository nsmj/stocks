using Microsoft.AspNetCore.Mvc;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Controllers;

public class ArquivosController(
    ILogger<ArquivosController> logger,
    BancoContext db,
    IConfiguration configuration,
    FileProcessor fileProcessor
) : Controller
{
    public IActionResult Importar()
    {
        return View();
    }

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
