using Stocks.BoQueries;

namespace Stocks.Bo;

public class IrpfRowsBuilder
{
    public static Dictionary<string, IrpfRow> BuildSwingTradeRows(
        List<SwingTradeResult> swingTradeResults,
        string ano
    )
    {
        Dictionary<string, IrpfRow> irpfRows = [];

        foreach (var row in swingTradeResults)
        {
            if (row.Ano == ano && row.Mes != null)
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

        return irpfRows;
    }
}
