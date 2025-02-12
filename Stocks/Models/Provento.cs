namespace Stocks.Models;

public partial class Provento
{
    public int Id { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public int AtivoId { get; set; }

    public virtual Ativo Ativo { get; set; }
}
