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
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ArquivosController> _logger = logger;
    private readonly BancoContext _db = db;
    private readonly FileProcessor _fileProcessor = fileProcessor;

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
            await _fileProcessor.ProcessarArquivos(_db, configuration, arquivo);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar o arquivo.");
            return StatusCode(500, "Erro ao processar o arquivo.");
        }
    }
}
