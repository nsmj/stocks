using Stocks.Extraction;

namespace Stocks.Tests;

public class NuinvestTest : BaseTest
{
    public NuinvestTest()
        : base(new Nuinvest()) { }

    [Fact]
    public async Task ExtrairDataNotaCorretagem()
    {
        await NotaCorretagem.ExtraiDadosDoArquivo(
            Db,
            "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf"
        );

        Assert.Equal(NotaCorretagem.Data, new DateTime(2019, 11, 4));
    }
}
