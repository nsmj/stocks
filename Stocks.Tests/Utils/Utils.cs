using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stocks.Data;

namespace Stocks.Tests;

public static class Utils
{
    public static BancoContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BancoContext>()
            .UseSqlite("Data Source=Data/BancoTeste.db")
            .Options;

        return new BancoContext(options);
    }

    public static IConfigurationRoot GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Define o diretório base
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true); // Adiciona o arquivo JSON

        return builder.Build(); // Constrói a configuração
    }
}
