using Microsoft.AspNetCore.Mvc;
using Stocks.Data;

namespace Stocks.Extraction.Strategies;

/// <summary>
/// Fábrica para criar estratégias de importação com base no tipo de pasta.
/// </summary>
public class EstrategiaImportacaoFactory(
    [FromServices] BancoContext db,
    [FromServices] IConfiguration configuration,
    [FromServices] PdfExtractor pdfExtractor
)
{
    /// <summary>
    /// Cria a estratégia apropriada com base no nome da pasta.
    /// </summary>
    /// <param name="nomePasta">Nome da pasta (ex: "NotasCorretagem", "Json")</param>
    /// <returns>A estratégia correspondente, ou null se não encontrar.</returns>
    public IEstrategiaImportacao? CriarEstrategia(string nomePasta)
    {
        return nomePasta switch
        {
            "NotasCorretagem" => new NotasCorretagemStrategy(db, configuration, pdfExtractor),
            "Json" => new JsonStrategy(db),
            _ => null,
        };
    }

    /// <summary>
    /// Obtém todas as estratégias disponíveis.
    /// </summary>
    /// <returns>Lista de estratégias criadas.</returns>
    public List<IEstrategiaImportacao> ObterTodasEstrategias()
    {
        return [new NotasCorretagemStrategy(db, configuration, pdfExtractor), new JsonStrategy(db)];
    }
}
