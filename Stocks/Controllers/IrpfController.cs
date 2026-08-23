using Microsoft.AspNetCore.Mvc;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Interfaces;
using Stocks.ViewModels;

namespace Stocks.Controllers;

/// <summary>
/// Controller responsável por gerenciar as operações relacionadas ao IRPF.
/// </summary>
public class IrpfController(
    [FromKeyedServices("SwingTrade")] IOperacaoListable swingTradeObj,
    [FromKeyedServices("DayTrade")] IOperacaoListable dayTradeObj,
    [FromKeyedServices("Fii")] IOperacaoListable fiiObj,
    LucroVendasAbaixo20kQuery lucroVendasAbaixo20kQuery,
    IrrfQuery irrfQuery,
    IrrfService irrfService,
    PosicaoFimAnoQuery posicaoFimAnoQuery,
    IrpfRowsBuilder irpfRowsBuilder,
    ICalcularPrejuizoAcumuladoUseCase calcularPrejuizoAcumuladoUseCase
) : Controller
{
    /// <summary>
    /// Exibe a página de IRPF com os dados filtrados por ano.
    /// </summary>

    /// <param name="ano"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(string ano)
    {
        IrpfViewModel irpfViewModel = new();

        if (ano is not null)
        {
            var dadosSwingTrade = await swingTradeObj.ResultadoOperacaoMesQueryAsync();
            var dadosDayTrade = await dayTradeObj.ResultadoOperacaoMesQueryAsync();
            var dadosFii = await fiiObj.ResultadoOperacaoMesQueryAsync();

            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k = await lucroVendasAbaixo20kQuery.ExecutarAsync(ano);
            irpfViewModel.SwingTradeRows = irpfRowsBuilder.Build(dadosSwingTrade, ano);
            irpfViewModel.DayTradeRows = irpfRowsBuilder.Build(dadosDayTrade, ano);
            irpfViewModel.FiiRows = irpfRowsBuilder.Build(dadosFii, ano);

            var irrfResults = await irrfQuery.ExecutarAsync(ano);

            irrfService.InjetarValoresIrrf(
                irpfViewModel.SwingTradeRows,
                irrfResults,
                "Swing Trade"
            );
            irrfService.InjetarValoresIrrf(irpfViewModel.DayTradeRows, irrfResults, "Day Trade");
            irrfService.InjetarValoresIrrf(irpfViewModel.FiiRows, irrfResults, "FII");

            irpfViewModel.PrejuizoAcumuladoAnoAnoAnteriorSwingTrade =
                calcularPrejuizoAcumuladoUseCase.InjetarPrejuizoAcumulado(
                    dadosSwingTrade,
                    irpfViewModel.SwingTradeRows,
                    ano
                );

            irpfViewModel.PrejuizoAcumuladoAnoAnteriorDayTrade =
                calcularPrejuizoAcumuladoUseCase.InjetarPrejuizoAcumulado(
                    dadosDayTrade,
                    irpfViewModel.DayTradeRows,
                    ano
                );

            irpfViewModel.PrejuizoAcumuladoAnoAnteriorFii =
                calcularPrejuizoAcumuladoUseCase.InjetarPrejuizoAcumulado(
                    dadosFii,
                    irpfViewModel.FiiRows,
                    ano
                );

            var PosicoesFimAno = await posicaoFimAnoQuery.ExecutarAsync(ano);
            irpfViewModel.PosicoesFimAno = [.. PosicoesFimAno];
        }

        return View(irpfViewModel);
    }
}
