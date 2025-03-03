using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.BoQueries;

public class IrrfResult
{
    public decimal Valor { get; set; }
    public int Mes { get; set; }
    public TipoOperacao? TipoOperacao { get; set; }
}

public class IrrfQueries
{
    public static async Task<List<IrrfResult>> IrrfQuery(BancoContext db, string ano)
    {
        var result = await db
            .Irrfs.GroupBy(i => new { Mes = i.Data.Month, i.TipoOperacao })
            .Where(g => g.Key.Mes >= 1 && g.Key.Mes <= 12 && g.First().Data.Year.ToString() == ano)
            .Select(g => new IrrfResult
            {
                Valor = Math.Round(g.Sum(i => i.Valor), 2),
                Mes = g.Key.Mes,
                TipoOperacao = g.Key.TipoOperacao,
            })
            .OrderBy(r => r.Mes)
            .ToListAsync();

        return result;
    }
}
