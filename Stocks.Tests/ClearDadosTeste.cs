using Stocks.Extraction;

namespace Stocks.Tests;

public class ClearDadosTeste
{
    private static Corretora Corretora => new Clear();

    public static TheoryData<Corretora, string, string> DadosExtrairDataNotaCorretagem =>
        [new(Corretora, "files_test/NotasCorretagem/Clear/20201202.pdf", "02/12/2020")];

    public static TheoryData<
        Corretora,
        string,
        int,
        int,
        decimal,
        decimal,
        decimal,
        int
    > DadosExtrairDadosNotaCorretagem =>
        new()
        {
            {
                Corretora,
                "files_test/NotasCorretagem/Clear/20201202.pdf",
                1,
                14,
                14.53m,
                203.42m,
                0.06m,
                0
            }, // Operações comuns.
            {
                Corretora,
                "files_test/NotasCorretagem/Clear/20210715.pdf",
                1,
                100,
                31.68m,
                3168,
                0.73m,
                0
            }, // Day trade.
        };

    public static TheoryData<
        Corretora,
        string,
        decimal,
        string,
        string
    > DadosExtrairIrrfNotaCorretagem =>
        [
            new(
                Corretora,
                "files_test/NotasCorretagem/Clear/20210706.pdf",
                0.23m,
                "06/07/2021",
                "Swing Trade"
            ), // Swing trade.
            new(
                Corretora,
                "files_test/NotasCorretagem/Clear/20210728.pdf",
                1.26m,
                "28/07/2021",
                "Day Trade"
            ), // Day Trade.
            new(
                Corretora,
                "files_test/NotasCorretagem/Clear/20210201.pdf",
                0.07m,
                "01/02/2021",
                "FII"
            ), // FII.
        ];
}
