using Microsoft.Extensions.Configuration;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Tests;

public class BaseTest
{
    protected IConfigurationRoot Configuration { get; }
    protected BancoContext Db { get; }
    protected NotaNegociacao NotaCorretagem { get; }

    public BaseTest(Corretora corretora)
    {
        Configuration = Utils.GetConfiguration();
        Db = Utils.GetDbContext();
        NotaCorretagem = new(corretora, Configuration);
    }
}
