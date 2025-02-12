using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stocks.Data;
using Stocks.Extraction;
using Stocks.Models;

namespace Stocks.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HomeController> _logger;
    private BancoContext _db;

    public HomeController(
        ILogger<HomeController> logger,
        BancoContext db,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _db = db;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public ActionResult ProcessarArquivos()
    {
        FileProcessor fileProcessor = new();

        DbConnection.Reset();

        fileProcessor.ImportarNotasCorretagem(_db, _configuration);

        fileProcessor.ImportarArquivosJson(_db);

        FileProcessor.CalcularResultados(_db);

        return Content("teste");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
