using Stocks.Extraction;

namespace Stocks.Tests;

public class ClearTest : BaseTest
{
    public ClearTest()
        : base(new Clear()) { }

    [Fact]
    public async Task ExtrairDataNotaCorretagem()
    {
        await NotaCorretagem.ExtraiDadosDoArquivo(
            Db,
            "files_test/NotasCorretagem/Clear/20201202.pdf"
        );

        Assert.Equal(NotaCorretagem.Data, new DateTime(2020, 12, 2));
    }

    [Theory]
    [InlineData("files_test/NotasCorretagem/Clear/20201202.pdf", 1, 14, 14.53, 203.42, 0.06)] // Operações comuns.
    [InlineData("files_test/NotasCorretagem/Clear/20210715.pdf", 1, 100, 31.68, 3168, 0.73)] // Day Trade.
    public async Task ExtrairDadosNotaCorretagem(
        string filePath,
        int compra,
        int quantidade,
        decimal precoAtivo,
        decimal valorTotal,
        decimal taxas
    )
    {
        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        var operacao = NotaCorretagem.Operacoes.First();

        // TODO: Verificar se dá de converter o campo Compra pra BOOL.
        Assert.Equal(compra, operacao.Compra);
        Assert.Equal(quantidade, operacao.Quantidade);
        Assert.Equal(precoAtivo, operacao.PrecoAtivo);
        Assert.Equal(valorTotal, operacao.ValorTotal);
        Assert.Equal(taxas, operacao.Taxas);
    }

    [Theory]
    [InlineData("files_test/NotasCorretagem/Clear/20210706.pdf", 0.23, "06/07/2021", "Swing Trade")] // Swing trade.
    [InlineData("files_test/NotasCorretagem/Clear/20210728.pdf", 1.26, "28/07/2021", "Day Trade")] // Day Trade.
    [InlineData("files_test/NotasCorretagem/Clear/20210201.pdf", 0.07, "01/02/2021", "FII")] // FII.
    public async Task ExtraiIrrf(string filePath, decimal valor, string data, string tipoOperacao)
    {
        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        var irrf = NotaCorretagem.Irrfs.First();

        Assert.Equal(valor, irrf.Valor);
        Assert.Equal(data, irrf.Data.ToString("dd/MM/yyyy"));
        Assert.Equal(tipoOperacao, irrf.TipoOperacao.Nome);
    }
}
