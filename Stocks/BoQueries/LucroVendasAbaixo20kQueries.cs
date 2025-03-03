using Microsoft.EntityFrameworkCore;
using Stocks.Data;

namespace Stocks.BoQueries;

public class LucroVendasAbaixo20kResult
{
    public decimal Valor { get; set; }
}

public class LucroVendasAbaixo20kQueries
{
    public static async Task<decimal> LucroVendasAbaixo20kQuery(BancoContext db, string ano)
    {
        var anoInicio = $"{ano}-01-01";
        var anoFim = $"{ano}-12-31";

        var result = await db
            .Database.SqlQuery<LucroVendasAbaixo20kResult>(
                @$"
            SELECT
            PRINTF('%.2f', SUM(lucro)) AS valor
            FROM
            (
            SELECT
                STRFTIME('%m', data) mes,
                SUM(lucro_liquido) lucro,
                SUM(valor_total) valor_vendas
            FROM
                operacao o
            LEFT JOIN ativo a
            ON o.ativo_id = a.id
            WHERE
                data BETWEEN {anoInicio} AND {anoFim}
            AND compra = 0
            AND lucro_liquido > 0
            AND a.tipo_ativo_id = 1
            AND o.tipo_operacao_id = 1
            GROUP BY mes
            HAVING valor_vendas < 20000
            )"
            )
            .SingleAsync();

        return result.Valor;
    }
}
