using Stocks.Extraction;

namespace Stocks.Tests;

public class NuinvestDadosTeste
{
    private static Corretora Corretora => new Nuinvest();

    public static TheoryData<Corretora, string, string> DadosExtrairDataNotaCorretagem =>
        [
            new(
                Corretora,
                "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
                "04/11/2019"
            ),
        ];

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
                "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
                1,
                76,
                13.84m,
                1051.84m,
                0.32m,
                0
            },
            {
                Corretora,
                "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
                1,
                1,
                13.82m,
                13.82m,
                0.00m,
                1
            },
            {
                Corretora,
                "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
                0,
                6,
                92.54m,
                555.24m,
                0.17m,
                2
            },
            {
                Corretora,
                "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
                0,
                5,
                103.07m,
                515.35m,
                0.16m,
                3
            },
        };
}
