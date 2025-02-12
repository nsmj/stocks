using System.Text.Json;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction;

public class JsonInformation
{
    public IList<Evento> Eventos { get; set; } = [];
    public IList<Operacao> Operacoes { get; set; } = [];

    public (List<Operacao>, List<Evento>) ExtrairDadosArquivo(BancoContext db, string path)
    {
        var jf = JsonSerializer.Deserialize<JsonInformation>(File.ReadAllText(path));

        List<Operacao> operacoes = [];
        List<Evento> eventos = [];

        if (jf != null)
        {
            if (jf.Eventos is not null)
            {
                foreach (Evento evento in jf.Eventos)
                {
                    evento.CompletarCampos(db);
                    eventos.Add(evento);
                }
            }

            if (jf.Operacoes is not null)
            {
                foreach (Operacao operacao in jf.Operacoes)
                {
                    operacao.CompletarCampos(db);
                    operacoes.Add(operacao);
                }
            }
        }

        return (operacoes, eventos);
    }
}

public class EventoJson
{
    public string CodigoAtivo { get; set; } = "";

    public int Fator { get; set; } = 0;

    public string DataEvento { get; set; } = "";

    public string Tipo { get; set; } = "";

    public decimal? Valor { get; set; }
}

public class OperacaoJson
{
    public string CodigoAtivo { get; set; } = "";

    public string DataOperacao { get; set; } = "";

    public bool Compra { get; set; }

    public int Quantidade { get; set; }

    public decimal PrecoAtivo { get; set; }

    public decimal ValorTotal { get; set; }

    public string Tipo { get; set; } = "";
}
