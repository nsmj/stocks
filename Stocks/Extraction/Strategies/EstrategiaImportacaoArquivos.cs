namespace Stocks.Extraction.Strategies;

/// <summary>
/// Classe abstrata genérica que define a estratégia para importação de arquivos.
/// Implementa o padrão Template Method com o algoritmo comum de importação.
/// </summary>
/// <typeparam name="T">Tipo de dados retornado pela extração.</typeparam>
public abstract class EstrategiaImportacaoArquivos<T> : IEstrategiaImportacao
{
    /// <summary>
    /// Nome da pasta que esta estratégia processa.
    /// </summary>
    public abstract string NomePasta { get; }

    /// <summary>
    /// Executa a importação dos arquivos (Template Method).
    /// Define o fluxo comum: extrair, processar e salvar dados.
    /// </summary>
    /// <param name="caminhoArquivos">Caminho da pasta contendo os arquivos a serem importados.</param>
    public async Task ExecutarAsync(string caminhoArquivos)
    {
        string[] files = Directory.GetFiles(caminhoArquivos, "*", SearchOption.AllDirectories);
        List<Task<T>> tarefasExtracao = [];

        // Extrai dados de cada arquivo
        foreach (var file in files)
        {
            tarefasExtracao.Add(ExtrairDadosArquivoAsync(file));
        }

        // Aguarda todas as extrações
        var dados = await Task.WhenAll(tarefasExtracao);

        // Salva os dados extraídos
        foreach (var dado in dados)
        {
            await SalvarDadosAsync(dado);
        }

        // Finaliza a operação (ex: SaveChangesAsync no EF)
        await FinalizarAsync();
    }

    /// <summary>
    /// Extrai dados de um arquivo específico. Deve ser implementado pelas estratégias concretas.
    /// </summary>
    /// <param name="caminhoArquivo">Caminho do arquivo a ser processado.</param>
    /// <returns>Objeto contendo os dados extraídos.</returns>
    protected abstract Task<T> ExtrairDadosArquivoAsync(string caminhoArquivo);

    /// <summary>
    /// Salva os dados extraídos no banco de dados. Deve ser implementado pelas estratégias concretas.
    /// </summary>
    /// <param name="dados">Dados extraídos a serem salvos.</param>
    protected abstract Task SalvarDadosAsync(T dados);

    /// <summary>
    /// Finaliza a operação (ex: commit de transação, SaveChangesAsync). Deve ser implementado pelas estratégias concretas.
    /// </summary>
    protected abstract Task FinalizarAsync();
}
