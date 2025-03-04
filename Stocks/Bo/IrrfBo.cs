using Microsoft.EntityFrameworkCore;
using Stocks.BoQueries;
using Stocks.Data;

namespace Stocks.Bo;

public class IrrfResult
{
    public decimal Valor { get; set; }
    public int Mes { get; set; }
    public string? NomeTipoOperacao { get; set; }
}

public class IrrfBo
{
    public async Task<List<IrrfResult>> IrrfQuery(BancoContext db, string ano)
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

    public void InjetarValoresIrrf(
        Dictionary<int, IrpfRowBo> irpfRows,
        List<IrrfResult> irrfResults,
        string nomeTipoOperacao
    )
    {
        foreach (var irrfResult in irrfResults)
        {
            if (irrfResult.NomeTipoOperacao != nomeTipoOperacao)
            {
                continue;
            }

            irpfRows[irrfResult.Mes].Irrf = irrfResult.Valor;
        }
    }
}
