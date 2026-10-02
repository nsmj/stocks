using Stocks.Interfaces;

namespace Stocks.Interfaces;

public interface IEstrategiaImportacaoFactory
{
    IEstrategiaImportacao? CriarEstrategia(string nomePasta);
    List<IEstrategiaImportacao> ObterTodasEstrategias();
}
