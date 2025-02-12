namespace Stocks.Models;

public partial class TipoOperacao
{
    public int Id { get; set; }

    public string Nome { get; set; }

    public virtual ICollection<Irrf> Irrfs { get; set; } = [];

    public virtual ICollection<Operacao> Operacoes { get; set; } = [];
}
