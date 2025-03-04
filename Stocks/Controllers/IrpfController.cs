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
            var dadosSwingTrade = await SwingTradeBoQueries.SwingTradeQuery(_db);
            var dadosDayTrade = await DayTradeBoQueries.DayTradeQuery(_db);

            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k =
                await LucroVendasAbaixo20kQueries.LucroVendasAbaixo20kQuery(_db, ano);
            irpfViewModel.SwingTradeRows = IrpfRowsBuilder.BuildIrpfRowsBo(dadosSwingTrade, ano);
            irpfViewModel.DayTradeRows = IrpfRowsBuilder.BuildIrpfRowsBo(dadosDayTrade, ano);

            var irrfResults = await IrrfBo.IrrfQuery(_db, ano);
            IrrfBo.InjetarValoresIrrf(irpfViewModel.SwingTradeRows, irrfResults, "Swing Trade");
            IrrfBo.InjetarValoresIrrf(irpfViewModel.DayTradeRows, irrfResults, "Day Trade");

            irpfViewModel.PrejuizoAcumuladoAnoAnoAnteiorSwingTrade =
                CalculadoraPrejuizoAcumuladoBo.InjetarPrejuizoAcumulado(
                    dadosSwingTrade,
                    irpfViewModel.SwingTradeRows,
                    ano
                );
        }

        return View(irpfViewModel);
    }
}
