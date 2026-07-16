using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;

namespace Stocks.Bo;

public class IrrfService(BancoContext db)
{
    public void InjetarValoresIrrf(
        Dictionary<int, IrpfRowDTO> irpfRows,
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
