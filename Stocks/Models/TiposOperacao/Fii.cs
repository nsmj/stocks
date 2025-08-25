using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Interfaces;

namespace Stocks.Models.TiposOperacao;

public class Fii : TipoOperacao, IOperacaoListable
{
    public async Task<List<ResultadoOperacaoMesDTO>> ResultadoOperacaoMesQuery(BancoContext db)
    {
        return await db
            .Database.SqlQuery<ResultadoOperacaoMesDTO>(
                $@"
                SELECT
                    STRFTIME('%Y', data) ano,
                    STRFTIME('%m', data) mes,
                    SUM(lucro_liquido) AS valor
                FROM
                    operacao o
                WHERE
                    o.tipo_operacao_id = 3
                    AND compra = 0
                GROUP BY ano, mes
            "
            )
            .ToListAsync();
    }
}
