namespace Stocks.Models;

public partial class TipoAtivo
{
    public int Id { get; set; }

    public string Nome { get; set; }

    public virtual ICollection<Ativo> Ativos { get; set; } = [];
}
