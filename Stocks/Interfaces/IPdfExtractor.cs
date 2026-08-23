namespace Stocks.Interfaces;

public interface IPdfExtractor
{
    /// <summary>
    /// Extrai os dados do PDF da nota de negociação.
    /// </summary>
    /// <param name="caminhoArquivo"></param>
    /// <param name="senhaArquivo"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    string[] ExtrairDadosPdf(string caminhoArquivo, string senhaArquivo);
}
