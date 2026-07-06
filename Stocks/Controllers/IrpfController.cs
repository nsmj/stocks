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
    LucroVendasAbaixo20kBo lucroVendasAbaixo20kBo,
    IrrfBo irrfBo,
    PosicaoFimAnoBo posicaoFimAnoBo,
    IrpfRowsBuilder irpfRowsBuilder,
    CalculadoraPrejuizoAcumuladoBo calculadoraPrejuizoAcumuladoBo
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
            var dadosSwingTrade = await swingTradeObj.ResultadoOperacaoMesQuery();
            var dadosDayTrade = await dayTradeObj.ResultadoOperacaoMesQuery();
            var dadosFii = await fiiObj.ResultadoOperacaoMesQuery();

            irpfViewModel.AnoFiltrado = ano;
            irpfViewModel.LucroVendasAbaixo20k =
                await lucroVendasAbaixo20kBo.LucroVendasAbaixo20kQuery(ano);
            irpfViewModel.SwingTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosSwingTrade, ano);
            irpfViewModel.DayTradeRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosDayTrade, ano);
            irpfViewModel.FiiRows = irpfRowsBuilder.BuildIrpfRowsBo(dadosFii, ano);

            var irrfResults = await irrfBo.IrrfQuery(ano);
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

            var PosicoesFimAno = await posicaoFimAnoBo.PosicaoFimAnoQuery(ano);
            irpfViewModel.PosicoesFimAno = [.. PosicoesFimAno];
        }

        return View(irpfViewModel);
    }
}
