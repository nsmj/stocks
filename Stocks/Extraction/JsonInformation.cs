using System.Text.Json;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Models;

namespace Stocks.Extraction;

public class JsonInformation(BancoContext db)
{
    public async Task<DadosArquivoJsonDto> ExtrairDadosArquivoAsync(string path)
    {
        var json = File.ReadAllText(path);
        var jf = JsonSerializer.Deserialize<DadosArquivoJsonDto>(json);

        List<Operacao> operacoes = [];
        List<Evento> eventos = [];

        if (jf is not null)
        {
            if (jf.Eventos is not null)
            {
                foreach (Evento evento in jf.Eventos)
                {
                    evento.CompletarCamposAsync(db);
                    eventos.Add(evento);
                }
            }

            if (jf.Operacoes is not null)
            {
                foreach (Operacao operacao in jf.Operacoes)
                {
                    operacao.CompletarCamposAsync(db);
                    operacoes.Add(operacao);
                }
            }
        }

        return new DadosArquivoJsonDto(operacoes, eventos);
    }
}
