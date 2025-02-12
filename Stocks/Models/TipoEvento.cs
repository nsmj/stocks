using System.ComponentModel.DataAnnotations;

namespace Stocks.Models;

public partial class TipoEvento
{
    [Key]
    public int Id { get; set; }

    public string Nome { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = [];
}
