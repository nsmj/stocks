using Stocks.Bo;
using Stocks.DTOs;

namespace Stocks.Interfaces;

public interface IIrrfService
{
    void InjetarValoresIrrf(
        Dictionary<int, IrpfRowDTO> irpfRows,
        List<IrrfResult> irrfResults,
        string nomeTipoOperacao
    );
}
