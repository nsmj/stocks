using System.Text.Json;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Models;

namespace Stocks.Extraction;

public class JsonInformation(BancoContext db)
{
    public IList<Evento> Eventos { get; set; } = [];
    public IList<Operacao> Operacoes { get; set; } = [];

    public async Task<DadosArquivoJsonDto> ExtrairDadosArquivo(string path)
    {
        var json = File.ReadAllText(path);
        var jf = JsonSerializer.Deserialize<JsonInformationDto>(json);

        List<Operacao> operacoes = [];
        List<Evento> eventos = [];

        if (jf is not null)
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

        return new DadosArquivoJsonDto(operacoes, eventos);
    }

    private record JsonInformationDto
    {
        public IList<Evento> Eventos { get; init; } = [];
        public IList<Operacao> Operacoes { get; init; } = [];
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
