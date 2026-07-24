namespace Stocks.Extraction.Strategies;

/// <summary>
/// Interface que define o contrato para estratégias de importação.
/// </summary>
public interface IEstrategiaImportacao
{
    /// <summary>
    /// Nome da pasta que esta estratégia processa.
    /// </summary>
    string NomePasta { get; }

    /// <summary>
    /// Executa a importação dos arquivos.
    /// </summary>
    /// <param name="caminhoArquivos">Caminho da pasta contendo os arquivos a serem importados.</param>
    Task ExecutarAsync(string caminhoArquivos);
}
