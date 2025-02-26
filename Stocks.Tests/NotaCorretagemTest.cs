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

    [Theory]
    [MemberData(
        nameof(ClearDadosTeste.DadosExtrairDataNotaCorretagem),
        MemberType = typeof(ClearDadosTeste)
    )]
    [MemberData(
        nameof(NuinvestDadosTeste.DadosExtrairDataNotaCorretagem),
        MemberType = typeof(NuinvestDadosTeste)
    )]
    public async Task ExtrairDataNotaCorretagem(Corretora corretora, string filePath, string data)
    {
        NotaCorretagem = new(corretora, Configuration);

        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        Assert.Equal(data, NotaCorretagem.Data.ToString("dd/MM/yyyy"));
    }

    [Theory]
    [MemberData(
        nameof(ClearDadosTeste.DadosExtrairDadosNotaCorretagem),
        MemberType = typeof(ClearDadosTeste)
    )]
    [MemberData(
        nameof(NuinvestDadosTeste.DadosExtrairDadosNotaCorretagem),
        MemberType = typeof(NuinvestDadosTeste)
    )]
    public async Task ExtrairDadosNotaCorretagem(
        Corretora corretora,
        string filePath,
        int compra,
        int quantidade,
        decimal precoAtivo,
        decimal valorTotal,
        decimal taxas,
        int indiceOperacao
    )
    {
        NotaCorretagem = new(corretora, Configuration);

        await NotaCorretagem.ExtraiDadosDoArquivo(Db, filePath);

        var operacao = NotaCorretagem.Operacoes[indiceOperacao];

        // TODO: Verificar se dá de converter o campo Compra pra BOOL.
        Assert.Equal(compra, operacao.Compra);
        Assert.Equal(quantidade, operacao.Quantidade);
        Assert.Equal(precoAtivo, operacao.PrecoAtivo);
        Assert.Equal(valorTotal, operacao.ValorTotal);
        Assert.Equal(taxas, operacao.Taxas);
    }

    [Theory]
    [MemberData(
        nameof(ClearDadosTeste.DadosExtrairIrrfNotaCorretagem),
        MemberType = typeof(ClearDadosTeste)
    )]
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
