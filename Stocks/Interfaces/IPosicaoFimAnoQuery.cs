using Stocks.ViewModels;

namespace Stocks.Interfaces;

public interface IPosicaoFimAnoQuery
{
    Task<IEnumerable<PosicaoFimAnoViewModel>> ExecutarAsync(string ano);
}
