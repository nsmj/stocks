using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stocks.Data;
using Stocks.Extraction;
using Stocks.Models;

namespace Stocks.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    BancoContext db,
    IConfiguration configuration
) : Controller
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<HomeController> _logger = logger;
    private BancoContext _db = db;

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
