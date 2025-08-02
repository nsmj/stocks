using Microsoft.AspNetCore.Mvc;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Interfaces;
using Stocks.ViewModels;

namespace Stocks.Controllers;

public class IrpfController(BancoContext db) : Controller
{
    public async Task<IActionResult> Index(
        [FromKeyedServices("SwingTrade")] IOperacaoListable swingTradeObj,
        [FromKeyedServices("DayTrade")] IOperacaoListable dayTradeObj,
        [FromKeyedServices("Fii")] IOperacaoListable fiiObj,
        LucroVendasAbaixo20kBo lucroVendasAbaixo20kBo,
        IrrfBo irrfBo,
        PosicaoFimAnoBo posicaoFimAnoBo,
        IrpfRowsBuilder irpfRowsBuilder,
        CalculadoraPrejuizoAcumuladoBo calculadoraPrejuizoAcumuladoBo,
        string ano
    )
    {
        IrpfViewModel irpfViewModel = new();

        if (ano != null)
        {
            var dadosSwingTrade = await swingTradeObj.ResultadoOperacaoMesQuery(db);
            var dadosDayTrade = await dayTradeObj.ResultadoOperacaoMesQuery(db);
            var dadosFii = await fiiObj.ResultadoOperacaoMesQuery(db);

            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k =
                await lucroVendasAbaixo20kBo.LucroVendasAbaixo20kQuery(db, ano);
            irpfViewModel.SwingTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosSwingTrade, ano);
            irpfViewModel.DayTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosDayTrade, ano);
            irpfViewModel.FiiRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosFii, ano);

            var irrfResults = await irrfBo.IrrfQuery(db, ano);
            irrfBo.InjetarValoresIrrf(irpfViewModel.SwingTradeRows, irrfResults, "Swing Trade");
            irrfBo.InjetarValoresIrrf(irpfViewModel.DayTradeRows, irrfResults, "Day Trade");
            irrfBo.InjetarValoresIrrf(irpfViewModel.FiiRows, irrfResults, "FII");

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

            irpfViewModel.PrejuizoAcumuladoAnoAnteriorFii =
                calculadoraPrejuizoAcumuladoBo.InjetarPrejuizoAcumulado(
                    dadosFii,
                    irpfViewModel.FiiRows,
                    ano
                );

            var PosicoesFimAno = await posicaoFimAnoBo.PosicaoFimAnoQuery(db, ano);
            irpfViewModel.PosicoesFimAno = [.. PosicoesFimAno];
        }

        return View(irpfViewModel);
    }
}
