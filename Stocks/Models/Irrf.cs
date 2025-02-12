namespace Stocks.Models;

public partial class Irrf
{
    public int Id { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public int TipoOperacaoId { get; set; }

    public virtual TipoOperacao TipoOperacao { get; set; }
}
