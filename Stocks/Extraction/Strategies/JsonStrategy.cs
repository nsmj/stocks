using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;

namespace Stocks.Extraction.Strategies;

/// <summary>
/// Estratégia para importação de arquivos JSON.
/// </summary>
public class JsonStrategy(BancoContext db) : EstrategiaImportacaoArquivos<DadosArquivoJsonDto>
{
    public override string NomePasta => "Json";

    protected override async Task<DadosArquivoJsonDto> ExtrairDadosArquivoAsync(
        string caminhoArquivo
    )
    {
        JsonInformation jsonInformation = new(db);
        return await jsonInformation.ExtrairDadosArquivoAsync(caminhoArquivo);
    }

    protected override async Task SalvarDadosAsync(DadosArquivoJsonDto dados)
    {
        await db.Operacoes.AddRangeAsync(dados.Operacoes);
        await db.Eventos.AddRangeAsync(dados.Eventos);
    }

    protected override async Task FinalizarAsync()
    {
        await db.SaveChangesAsync();
    }
}
