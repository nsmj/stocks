using Stocks.BoQueries;

namespace Stocks.Bo;

public class IrpfRowsBuilder
{
    public Dictionary<int, IrpfRowBo> BuildIrpfRowsBo(
        List<ResultadoOperacaoMesBo> results,
        string ano
    )
    {
        Dictionary<int, IrpfRowBo> irpfRows = [];

        foreach (var row in results)
        {
            if (row.Ano == int.Parse(ano))
            {
                irpfRows[row.Mes] = new IrpfRowBo
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
                irpfRows[i] = new IrpfRowBo
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
