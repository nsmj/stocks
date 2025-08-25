using Stocks.DTOs;

namespace Stocks.Bo;

public class IrpfRowsBuilder
{
    public Dictionary<int, IrpfRowDTO> BuildIrpfRowsBo(
        List<ResultadoOperacaoMesDTO> results,
        string ano
    )
    {
        Dictionary<int, IrpfRowDTO> irpfRows = [];

        foreach (var row in results)
        {
            if (row.Ano == int.Parse(ano))
            {
                irpfRows[row.Mes] = new IrpfRowDTO
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
                irpfRows[i] = new IrpfRowDTO
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
