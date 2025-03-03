using Microsoft.AspNetCore.Mvc;
using Stocks.Bo;
using Stocks.BoQueries;
using Stocks.Data;
using Stocks.ViewModels;

namespace Stocks.Controllers;

public class IrpfController : Controller
{
    private BancoContext _db;

    public IrpfController(BancoContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string ano)
    {
        IrpfViewModel irpfViewModel = new();

        if (ano != null)
        {
            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k =
                await LucroVendasAbaixo20kQueries.LucroVendasAbaixo20kQuery(_db, ano);
            irpfViewModel.SwingTradeRows = IrpfRowsBuilder.BuildIrpfRowsBo(
                SwingTradeBoQueries.SwingTradeQuery(_db),
                ano
            );
            irpfViewModel.DayTradeRows = IrpfRowsBuilder.BuildIrpfRowsBo(
                DayTradeBoQueries.DayTradeQuery(_db),
                ano
            );
        }

        return View(irpfViewModel);
    }
}
