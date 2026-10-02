using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Interfaces;

namespace Stocks.Bo;

public class IrrfService(BancoContext db) : IIrrfService
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
