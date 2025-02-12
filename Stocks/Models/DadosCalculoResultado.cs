namespace Stocks.Models;

public class DadosCalculoResultado
{
    public int Id { get; set; }

    public string? Tipo { get; set; }

    public string? Data { get; set; }

    public int? Fator { get; set; }

    public int? Quantidade { get; set; }

    public decimal? PrecoAtivo { get; set; }

    public decimal? ValorTotal { get; set; }

    public int? Compra { get; set; }

    public decimal? Taxas { get; set; }

    public int AtivoId { get; set; }

    public string? TipoOperacaoEvento { get; set; }
}
