namespace Stocks.Models;

/// <summary>
/// Represents calculation result data for stock operations.
/// </summary>
public record DadosCalculoResultadoDTO(
    int Id,
    string? Tipo,
    string? Data,
    int? Fator,
    int? Quantidade,
    decimal? PrecoAtivo,
    decimal? ValorTotal,
    int? Compra,
    decimal? Taxas,
    int AtivoId,
    string? TipoOperacaoEvento
);
