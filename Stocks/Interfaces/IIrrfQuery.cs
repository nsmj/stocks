using Stocks.Bo;

namespace Stocks.Interfaces;

public interface IIrrfQuery
{
    Task<List<IrrfResult>> ExecutarAsync(string ano);
}
