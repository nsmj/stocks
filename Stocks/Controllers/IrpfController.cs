using Microsoft.AspNetCore.Mvc;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Interfaces;
using Stocks.ViewModels;

namespace Stocks.Controllers;

public class IrpfController : Controller
{
    private BancoContext _db;

    public IrpfController(BancoContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(
        [FromKeyedServices("SwingTrade")] IOperacaoListable swingTradeObj,
        [FromKeyedServices("DayTrade")] IOperacaoListable dayTradeObj,
        LucroVendasAbaixo20kBo lucroVendasAbaixo20kBo,
        IrrfBo irrfBo,
        IrpfRowsBuilder irpfRowsBuilder,
        CalculadoraPrejuizoAcumuladoBo calculadoraPrejuizoAcumuladoBo,
        string ano
    )
    {
        IrpfViewModel irpfViewModel = new();

        if (ano != null)
        {
            var dadosSwingTrade = await swingTradeObj.ResultadoOperacaoMesQuery(_db);
            var dadosDayTrade = await dayTradeObj.ResultadoOperacaoMesQuery(_db);

            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k =
                await lucroVendasAbaixo20kBo.LucroVendasAbaixo20kQuery(_db, ano);
            irpfViewModel.SwingTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosSwingTrade, ano);
            irpfViewModel.DayTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosDayTrade, ano);

            var irrfResults = await irrfBo.IrrfQuery(_db, ano);
            irrfBo.InjetarValoresIrrf(irpfViewModel.SwingTradeRows, irrfResults, "Swing Trade");
            irrfBo.InjetarValoresIrrf(irpfViewModel.DayTradeRows, irrfResults, "Day Trade");

            irpfViewModel.PrejuizoAcumuladoAnoAnoAnteriorSwingTrade =
                calculadoraPrejuizoAcumuladoBo.InjetarPrejuizoAcumulado(
                    dadosSwingTrade,
                    irpfViewModel.SwingTradeRows,
                    ano
                );

            irpfViewModel.PrejuizoAcumuladoAnoAnteriorDayTrade =
                calculadoraPrejuizoAcumuladoBo.InjetarPrejuizoAcumulado(
                    dadosDayTrade,
                    irpfViewModel.DayTradeRows,
                    ano
                );
        }

        return View(irpfViewModel);
    }
}
