using Stocks.Bo;
using Stocks.BoQueries;

namespace Stocks.Tests;

public class SwingTradeTest
{
    private readonly List<SwingTradeResult> swingTradeResults =
    [
        new SwingTradeResult
        {
            Ano = "2022",
            Mes = "01",
            Valor = -0.77m,
        },
        new SwingTradeResult
        {
            Ano = "2022",
            Mes = "02",
            Valor = -7.41m,
        },
        new SwingTradeResult
        {
            Ano = "2022",
            Mes = "05",
            Valor = -0.04m,
        },
        new SwingTradeResult
        {
            Ano = "2022",
            Mes = "11",
            Valor = -11.69m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "01",
            Valor = -10.38m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "02",
            Valor = -0.36m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "04",
            Valor = -11.88m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "05",
            Valor = -46.65m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "06",
            Valor = -13.74m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "08",
            Valor = -526.88m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "09",
            Valor = -98.11m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "10",
            Valor = -45.0m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "11",
            Valor = 54.23m,
        },
        new SwingTradeResult
        {
            Ano = "2023",
            Mes = "12",
            Valor = -13.34m,
        },
    ];

    [Fact]
    public void BuildSwingTradeRows()
    {
        var irpfRows = IrpfRowsBuilder.BuildSwingTradeRows(swingTradeResults.AsQueryable(), "2022");

        Assert.Equal(4, irpfRows.Count);
        Assert.Equal(-0.77m, irpfRows["01"].Total);
        Assert.Equal(-7.41m, irpfRows["02"].Total);
        Assert.Equal(-0.04m, irpfRows["05"].Total);
        Assert.Equal(-11.69m, irpfRows["11"].Total);

        irpfRows = IrpfRowsBuilder.BuildSwingTradeRows(swingTradeResults.AsQueryable(), "2023");

        Assert.Equal(10, irpfRows.Count);
        Assert.Equal(-10.38m, irpfRows["01"].Total);
        Assert.Equal(-0.36m, irpfRows["02"].Total);
        Assert.Equal(-11.88m, irpfRows["04"].Total);
        Assert.Equal(-46.65m, irpfRows["05"].Total);
        Assert.Equal(-13.74m, irpfRows["06"].Total);
        Assert.Equal(-526.88m, irpfRows["08"].Total);
        Assert.Equal(-98.11m, irpfRows["09"].Total);
        Assert.Equal(-45.0m, irpfRows["10"].Total);
        Assert.Equal(54.23m, irpfRows["11"].Total);
        Assert.Equal(-13.34m, irpfRows["12"].Total);
    }
}
