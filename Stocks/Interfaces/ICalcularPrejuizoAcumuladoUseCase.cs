using Stocks.DTOs;

namespace Stocks.Interfaces;

public interface ICalcularPrejuizoAcumuladoUseCase
{
    /// <summary>
    /// Injeta o prejuízo acumulado nas operações financeiras para o ano especificado.
    /// </summary>
    /// <param name="resultadoOperacoes"></param>
    /// <param name="irpfRows"></param>
    /// <param name="ano"></param>
    /// <returns></returns>
    decimal InjetarPrejuizoAcumulado(
        List<ResultadoOperacaoMesDTO> resultadoOperacoes,
        Dictionary<int, IrpfRowDTO> irpfRows,
        string ano
    );
}
