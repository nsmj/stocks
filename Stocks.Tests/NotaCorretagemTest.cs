using Microsoft.Extensions.Configuration;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Tests;

public class NotaCorretagemTest
{
    protected IConfigurationRoot Configuration { get; }
    protected BancoContext Db { get; }
    protected NotaNegociacao? NotaCorretagem { get; set; }

    public NotaCorretagemTest()
    {
        Configuration = Utils.GetConfiguration();
        Db = Utils.GetDbContext();
    }

    public static IEnumerable<
        TheoryDataRow<Corretora, string, string>
    > DadosExtrairDataNotaCorretagem =
    [
        new(new Clear(), "files_test/NotasCorretagem/Clear/20201202.pdf", "02/12/2020"),
        new(
            new Nuinvest(),
            "files_test/NotasCorretagem/Nuinvest/20191104_Invoice_19267.pdf",
            "04/11/2019"
        ),
    ];

    public static IEnumerable<
        TheoryDataRow<Corretora, string, int, int, decimal, decimal, decimal>
    > DadosExtrairDadosNotaCorretagem =
    [
        new(
            new Clear(),
            "files_test/NotasCorretagem/Clear/20201202.pdf",
            1,
            14,
            14.53m,
            203.42m,
            0.06m
        ), // Operações comuns.
        new(
            new Clear(),
            "files_test/NotasCorretagem/Clear/20210715.pdf",
            1,
            100,
            31.68m,
            3168,
            0.73m
        ), // Day trade.
    ];

    public static IEnumerable<
        TheoryDataRow<Corretora, string, decimal, string, string>
    > DadosExtrairIrrfNotaCorretagem =
    [
        new(
            new Clear(),
            "files_test/NotasCorretagem/Clear/20210706.pdf",
            0.23m,
            "06/07/2021",
            "Swing Trade"
        ), // Swing trade.
        new(
            new Clear(),
            "files_test/NotasCorretagem/Clear/20210728.pdf",
            1.26m,
            "28/07/2021",
            "Day Trade"
        ), // Day Trade.
        new(
            new Clear(),
            "files_test/NotasCorretagem/Clear/20210201.pdf",
            0.07m,
            "01/02/2021",
            "FII"
        ), // FII.
    ];

    [Theory]
    [MemberData(nameof(DadosExtrairDataNotaCorretagem))]
    public async Task ExtrairDataNotaCorretagem(Corretora corretora, string filePath, string data)
    {
        NotaCorretagem = new(corretora, Configuration);

        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        Assert.Equal(data, NotaCorretagem.Data.ToString("dd/MM/yyyy"));
    }

    [Theory]
    [MemberData(nameof(DadosExtrairDadosNotaCorretagem))]
    public async Task ExtrairDadosNotaCorretagem(
        Corretora corretora,
        string filePath,
        int compra,
        int quantidade,
        decimal precoAtivo,
        decimal valorTotal,
        decimal taxas
    )
    {
        NotaCorretagem = new(corretora, Configuration);

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
    [MemberData(nameof(DadosExtrairIrrfNotaCorretagem))]
    public async Task ExtrairIrrfNotaCorretagem(
        Corretora corretora,
        string filePath,
        decimal valor,
        string data,
        string tipoOperacao
    )
    {
        NotaCorretagem = new NotaNegociacao(corretora, Configuration);

        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        var irrf = NotaCorretagem.Irrfs.First();

        Assert.Equal(valor, irrf.Valor);
        Assert.Equal(data, irrf.Data.ToString("dd/MM/yyyy"));
        Assert.Equal(tipoOperacao, irrf.TipoOperacao.Nome);
    }
}
