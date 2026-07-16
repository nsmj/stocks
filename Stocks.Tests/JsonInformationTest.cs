using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Tests;

public class JsonInformationTest
{
    protected BancoContext Db { get; }
    protected NotaNegociacao? NotaCorretagem { get; set; }

    public JsonInformationTest()
    {
        Db = Utils.GetDbContext();
    }

    [Fact]
    public async Task ExtrairEventosArquivoJson()
    {
        var arquivoJson = new JsonInformation(Db);

        var eventos = (
            await arquivoJson.ExtrairDadosArquivoAsync("files_test/Json/20200605.json")
        ).Eventos;

        var evento = eventos.First();

        Assert.Equal("05/06/2020", evento.Data.ToShortDateString());
        Assert.Equal(10, evento.Fator);
        Assert.Equal("Grupamento", evento.TipoEvento.Nome);
        Assert.Equal("TCSA3", evento.Ativo.Codigo);
    }

    [Fact]
    public async Task ExtrairOperacoesArquivoJson()
    {
        var arquivoJson = new JsonInformation(Db);

        var operacoes = (
            await arquivoJson.ExtrairDadosArquivoAsync("files_test/Json/20200403.json")
        ).Operacoes;

        var operacao = operacoes.First();

        Assert.Equal("03/04/2020", operacao.Data.ToShortDateString());
        Assert.Equal(3, operacao.Quantidade);
        Assert.Equal(2.97m, operacao.PrecoAtivo);
        Assert.Equal("Swing Trade", operacao.TipoOperacao.Nome);
        Assert.Equal("JHSF3", operacao.Ativo.Codigo);
        Assert.Equal(1, operacao.Compra);
        Assert.Equal(8.91m, operacao.ValorTotal);
    }
}
