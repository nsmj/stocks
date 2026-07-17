using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;

namespace Stocks.Bo;

public class IrrfResult
{
    public decimal Valor { get; set; }
    public int Mes { get; set; }
    public string? NomeTipoOperacao { get; set; }
}

public class IrrfQuery(BancoContext db)
{
    public async Task<List<IrrfResult>> ExecuteAsync(string ano)
    {
        var result = db.Database.SqlQuery<IrrfResult>(
            $@"
                SELECT ROUND(SUM(valor), 2) AS valor, STRFTIME('%m', data) mes, tipo_operacao.nome AS NomeTipoOperacao
                FROM irrf
                LEFT JOIN tipo_operacao ON irrf.tipo_operacao_id = tipo_operacao.id
                WHERE STRFTIME('%Y', data) = {ano}
                GROUP BY mes, NomeTipoOperacao"
        );

        return await result.ToListAsync();
    }
}
