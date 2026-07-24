using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;

namespace Stocks.Extraction.Strategies;

/// <summary>
/// Estratégia para importação de notas de corretagem.
/// </summary>
public class NotasCorretagemStrategy(
    BancoContext db,
    IConfiguration configuration,
    PdfExtractor pdfExtractor
) : EstrategiaImportacaoArquivos<DadosNotaNegociacaoDto>
{
    public override string NomePasta => "NotasCorretagem";

    protected override async Task<DadosNotaNegociacaoDto> ExtrairDadosArquivoAsync(
        string caminhoArquivo
    )
    {
        Regex rgx = new(Path.Combine("(.*)", "(.*)", "(.*)", "(.*)").Replace("\\", "\\\\"));
        Match matchObj = rgx.Match(caminhoArquivo);
        var strategy = matchObj.Groups[3].ToString();

        var corretora = Corretora.Factory(strategy);
        NotaNegociacao notaNegociacao = new(corretora, configuration, db, pdfExtractor);

        return await notaNegociacao.ExtraiDadosDoArquivoAsync(caminhoArquivo);
    }

    protected override async Task SalvarDadosAsync(DadosNotaNegociacaoDto dados)
    {
        await db.Operacoes.AddRangeAsync(dados.Operacoes);
        await db.Irrfs.AddRangeAsync(dados.Irrfs);
    }

    protected override async Task FinalizarAsync()
    {
        await db.SaveChangesAsync();
    }
}
