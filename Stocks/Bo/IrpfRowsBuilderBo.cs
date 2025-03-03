using Stocks.BoQueries;

namespace Stocks.Bo;

public class IrpfRowsBuilder
{
    public static Dictionary<int, IrpfRow> BuildIrpfRowsBo(
        IQueryable<ResultadoOperacaoMesBo> results,
        string ano
    )
    {
        Dictionary<int, IrpfRow> irpfRows = [];

        foreach (var row in results)
        {
            if (row.Ano == ano)
            {
                irpfRows[row.Mes] = new IrpfRow
                {
                    Total = row.Valor,
                    ImpostoPago = 0,
                    Irrf = 0,
                    PrejuizoAcumulado = 0,
                };
            }
        }

        // Preenche os meses que não tiveram operações com o valor 0.
        for (int i = 1; i <= 12; i++)
        {
            if (!irpfRows.ContainsKey(i))
            {
                irpfRows[i] = new IrpfRow
                {
                    Total = 0,
                    ImpostoPago = 0,
                    Irrf = 0,
                    PrejuizoAcumulado = 0,
                };
            }
        }

        return irpfRows;
    }
}
