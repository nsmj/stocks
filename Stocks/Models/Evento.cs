using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;

namespace Stocks.Models;

/// <summary>
/// Representa um evento financeiro associado a um ativo.
/// Um evento pode ser um desdobramento, grupamento, bonificação, etc.
/// </summary>
public partial class Evento
{
    public int Id { get; set; }

    public DateTime Data { get; set; }

    public int Fator { get; set; }

    public decimal? Valor { get; set; }

    public int TipoEventoId { get; set; }

    public int AtivoId { get; set; }

    public virtual Ativo Ativo { get; set; }

    public virtual TipoEvento TipoEvento { get; set; }

    [NotMapped]
    public string CodigoAtivo { get; set; }

    [NotMapped]
    public string Tipo { get; set; }

    [NotMapped]
    public string DataEvento { get; set; }

    /// <summary>
    /// Completa os campos necessários da entidade Evento.
    /// </summary>
    /// <param name="db"></param>
    public async void CompletarCampos(BancoContext db)
    {
        Data = DateTime.Parse(DataEvento);

        TipoEvento = await db.TiposEvento.SingleAsync(t => t.Nome == Tipo);
        Ativo = await db.Ativos.SingleAsync(a => a.Codigo == CodigoAtivo);
    }
}
