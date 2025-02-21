using Microsoft.Extensions.Configuration;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Tests;

public class ClearTest
{
    private IConfigurationRoot Configuration { get; }
    private BancoContext Db { get; }
    private NotaNegociacao NotaCorretagem { get; }

    public ClearTest()
    {
        Configuration = Utils.GetConfiguration();
        Db = Utils.GetDbContext();
        NotaCorretagem = new(new Clear(), Configuration);
    }

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
    [InlineData("files_test/NotasCorretagem/Clear/20201202.pdf", 1, 14, 14.53, 203.42, 0.06)]
    [InlineData("files_test/NotasCorretagem/Clear/20210715.pdf", 1, 100, 31.68, 3168, 0.73)]
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
}
