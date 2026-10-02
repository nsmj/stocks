using Stocks.DTOs;

namespace Stocks.Interfaces;

public interface IIrpfRowsBuilder
{
    Dictionary<int, IrpfRowDTO> Build(List<ResultadoOperacaoMesDTO> results, string ano);
}
