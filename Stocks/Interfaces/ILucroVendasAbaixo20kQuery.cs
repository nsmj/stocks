using Stocks.Bo;

namespace Stocks.Interfaces;

public interface ILucroVendasAbaixo20kQuery
{
    Task<decimal> ExecutarAsync(string ano);
}
