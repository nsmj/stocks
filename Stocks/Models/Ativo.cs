namespace Stocks.Models;

public partial class Ativo
{
    public int Id { get; set; }

    public string Codigo { get; set; }

    public string Nome { get; set; }

    public string DescricaoNota { get; set; }

    public string? TipoAcao { get; set; }

    public decimal? PrecoMedio { get; set; }

    public int? Posicao { get; set; }

    public string Cnpj { get; set; }

    public string? CnpjFontePagadora { get; set; }

    public decimal? FatorPrecoTeto { get; set; }

    public decimal? PrecoTetoMedio { get; set; }

    public decimal? PrecoTetoProjetivo { get; set; }

    public int TipoAtivoId { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = [];

    public virtual ICollection<Operacao> Operacoes { get; set; } = [];

    public virtual ICollection<PosicaoFimAno> PosicoesFimAno { get; set; } = [];

    public virtual ICollection<Provento> Proventos { get; set; } = [];

    public virtual TipoAtivo TipoAtivo { get; set; }
}
