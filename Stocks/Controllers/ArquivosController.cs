using Microsoft.AspNetCore.Mvc;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Controllers;

/// <summary>
/// Controller responsável por gerenciar o upload e processamento de arquivos financeiros.
/// </summary>
/// <param name="logger"></param>
/// <param name="posicaoFimAnoBo"></param>
/// <param name="fileProcessor"></param>
public class ArquivosController(
    ILogger<ArquivosController> logger,
    PosicaoFimAnoQuery posicaoFimAnoBo,
    [FromServices] ImportarArquivosUseCase importarArquivosUseCase
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
            await importarArquivosUseCase.ExecuteAsync(arquivo);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao importar o arquivo.");
            return StatusCode(500, "Erro ao processar o arquivo.");
        }
    }
}
