using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Extraction;

namespace Stocks.Models;

public partial class Operacao : IStorable
{
    public int Id { get; set; }

    public DateTime Data { get; set; }

    public int? Quantidade { get; set; }

    public decimal? PrecoAtivo { get; set; }

    public decimal? ValorTotal { get; set; }

    public decimal? Taxas { get; set; }

    public int Compra { get; set; }

    public decimal? LucroLiquido { get; set; }

    public int AtivoId { get; set; }

    public int TipoOperacaoId { get; set; }

    public virtual Ativo Ativo { get; set; }

    public virtual TipoOperacao TipoOperacao { get; set; }

    [NotMapped]
    public string CodigoAtivo { get; set; }

    [NotMapped]
    public string Tipo { get; set; }

    [NotMapped]
    public string DataOperacao { get; set; }

    public async void CompletarCampos(BancoContext db)
    {
        Data = DateTime.Parse(DataOperacao);

        TipoOperacao = await db.TiposOperacao.SingleAsync(t => t.Nome == Tipo);
        Ativo = await db.Ativos.SingleAsync(a => a.Codigo == CodigoAtivo);
    }
}
