using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stocks.Models;

namespace Stocks.Controllers;

/// <summary>
/// Controller para a página inicial do aplicativo.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Exibe a página inicial do aplicativo.
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
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
