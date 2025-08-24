using Stocks.BoQueries;

namespace Stocks.Bo;

/// <summary>
/// Calculadora de prejuízo acumulado para operações financeiras.
/// </summary>
public class CalculadoraPrejuizoAcumuladoBo
{
    /// <summary>
    /// Injeta o prejuízo acumulado nas operações financeiras para o ano especificado.
    /// </summary>
    /// <param name="resultadoOperacoes"></param>
    /// <param name="irpfRows"></param>
    /// <param name="ano"></param>
    /// <returns></returns>
    public decimal InjetarPrejuizoAcumulado(
        List<ResultadoOperacaoMesBo> resultadoOperacoes,
        Dictionary<int, IrpfRowBo> irpfRows,
        string ano
    )
    {
        decimal prejuizoAcumuladoAnoAnterior = 0;
        decimal prejuizoAcumulado = 0;

        foreach (var r in resultadoOperacoes)
        {
            if (r.Valor < 0)
            {
                prejuizoAcumulado += r.Valor;
            }
            else if (r.Valor > 0)
            {
                if (prejuizoAcumulado + r.Valor < 0)
                {
                    prejuizoAcumulado += r.Valor;
                }
                else
                {
                    prejuizoAcumulado = 0;
                }
            }

            // Obter prejuizo acumulado apenas do ano passado.
            if (r.Ano <= (int.Parse(ano) - 1))
            {
                prejuizoAcumuladoAnoAnterior = prejuizoAcumulado;
            }

            // Não mostrar dados de outros anos além do filtrado.
            if (r.Ano != int.Parse(ano))
            {
                continue;
            }

            irpfRows[r.Mes].PrejuizoAcumulado = prejuizoAcumulado;
        }

        return prejuizoAcumuladoAnoAnterior;
    }
}
