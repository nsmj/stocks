using Stocks.Bo;

namespace Stocks.Tests;

public class SwingTradeTest
{
    private readonly List<ResultadoOperacaoMesBo> ResultadosOperacaoMes =
    [
        new ResultadoOperacaoMesBo
        {
            Ano = "2022",
            Mes = 1,
            Valor = -0.77m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2022",
            Mes = 2,
            Valor = -7.41m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2022",
            Mes = 5,
            Valor = -0.04m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2022",
            Mes = 11,
            Valor = -11.69m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 1,
            Valor = -10.38m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 2,
            Valor = -0.36m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 4,
            Valor = -11.88m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 5,
            Valor = -46.65m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 6,
            Valor = -13.74m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 8,
            Valor = -526.88m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 9,
            Valor = -98.11m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 10,
            Valor = -45.0m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 11,
            Valor = 54.23m,
        },
        new ResultadoOperacaoMesBo
        {
            Ano = "2023",
            Mes = 12,
            Valor = -13.34m,
        },
    ];

    [Fact]
    public void BuildIrpfRows()
    {
        var irpfRows = IrpfRowsBuilder.BuildIrpfRowsBo(ResultadosOperacaoMes.AsQueryable(), "2022");

        Assert.Equal(12, irpfRows.Count);
        Assert.Equal(-0.77m, irpfRows[1].Total);
        Assert.Equal(-7.41m, irpfRows[2].Total);
        Assert.Equal(-0.04m, irpfRows[5].Total);
        Assert.Equal(-11.69m, irpfRows[11].Total);

        irpfRows = IrpfRowsBuilder.BuildIrpfRowsBo(ResultadosOperacaoMes.AsQueryable(), "2023");

        Assert.Equal(12, irpfRows.Count);
        Assert.Equal(-10.38m, irpfRows[1].Total);
        Assert.Equal(-0.36m, irpfRows[2].Total);
        Assert.Equal(-11.88m, irpfRows[4].Total);
        Assert.Equal(-46.65m, irpfRows[5].Total);
        Assert.Equal(-13.74m, irpfRows[6].Total);
        Assert.Equal(-526.88m, irpfRows[8].Total);
        Assert.Equal(-98.11m, irpfRows[9].Total);
        Assert.Equal(-45.0m, irpfRows[10].Total);
        Assert.Equal(54.23m, irpfRows[11].Total);
        Assert.Equal(-13.34m, irpfRows[12].Total);
    }
}
