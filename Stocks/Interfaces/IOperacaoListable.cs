using Stocks.Bo;
using Stocks.Data;

namespace Stocks.Interfaces;

public interface IOperacaoListable
{
    public Task<List<ResultadoOperacaoMesBo>> ResultadoOperacaoMesQuery(BancoContext db);
}
