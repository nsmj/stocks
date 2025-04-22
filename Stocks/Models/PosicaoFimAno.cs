namespace Stocks.Models;

public partial class PosicaoFimAno
{
    public int Id { get; set; }

    public int Ano { get; set; }

    public decimal PrecoMedio { get; set; }

    public decimal CustoTotal { get; set; }

    public int Posicao { get; set; }

    public int AtivoId { get; set; }

    public virtual Ativo Ativo { get; set; }
}
