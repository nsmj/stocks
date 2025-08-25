using Stocks.Data;
using Stocks.DTOs;

namespace Stocks.Interfaces;

public interface IOperacaoListable
{
    public Task<List<ResultadoOperacaoMesDTO>> ResultadoOperacaoMesQuery(BancoContext db);
}
