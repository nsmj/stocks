using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.Models;
using Stocks.ViewModels;

namespace Stocks.Bo;

/// <summary>
/// Resultado da consulta de posição de fim de ano.
/// </summary>
/// <param name="Codigo">Código do ativo.</param>
/// <param name="Cnpj">CNPJ do ativo.</param>
/// <param name="NomeAtivo">Nome do ativo.</param>
/// <param name="Posicao">Posição em quantidade.</param>
/// <param name="PrecoMedio">Preço médio.</param>
/// <param name="CustoTotal">Custo total.</param>
/// <param name="TipoAtivo">Tipo do ativo.</param>
public record PosicaoFimAnoResult(
    string Codigo,
    string Cnpj,
    string NomeAtivo,
    decimal Posicao,
    decimal PrecoMedio,
    decimal CustoTotal,
    string TipoAtivo
);

public class PosicaoFimAnoBo
{
    public async Task<IEnumerable<PosicaoFimAnoViewModel>> PosicaoFimAnoQuery(
        BancoContext db,
        string ano
    )
    {
        var resultados = await PosicaoFimAnoResults(db, ano);

        ICollection<PosicaoFimAnoViewModel> posicoesFimAno = [];

        foreach (var resultado in resultados)
        {
            PosicaoFimAnoViewModel posicaoFimAno = new() { Codigo = resultado.Codigo };

            switch (resultado.TipoAtivo)
            {
                case "Ação":
                    posicaoFimAno.Texto =
                        $"{resultado.Posicao} AÇÕES DA {resultado.NomeAtivo} CÓDIGO DE NEGOCIAÇÃO B3: {resultado.Codigo}. CNPJ {resultado.Cnpj}. PREÇO MÉDIO DE R$ {resultado.PrecoMedio} POR AÇÃO. CUSTO TOTAL DE R$ {resultado.CustoTotal}.";
                    break;
                case "FII":
                    posicaoFimAno.Texto =
                        $"{resultado.Posicao} COTAS DO FII {resultado.NomeAtivo} CÓDIGO DE NEGOCIAÇÃO B3: {resultado.Codigo}. CNPJ {resultado.Cnpj}. PREÇO MÉDIO DE R$ {resultado.PrecoMedio} POR COTA. CUSTO TOTAL DE R$ {resultado.CustoTotal}.";
                    break;
                case "ETF":
                    posicaoFimAno.Texto =
                        $"{resultado.Posicao} COTAS DO ETF {resultado.NomeAtivo} CÓDIGO DE NEGOCIAÇÃO B3: {resultado.Codigo}. CNPJ {resultado.Cnpj}. PREÇO MÉDIO DE R$ {resultado.PrecoMedio} POR COTA. CUSTO TOTAL DE R$ {resultado.CustoTotal}.";
                    break;
            }

            posicoesFimAno.Add(posicaoFimAno);
        }

        return posicoesFimAno;
    }

    private async Task<List<PosicaoFimAnoResult>> PosicaoFimAnoResults(BancoContext db, string ano)
    {
        return await db
            .Database.SqlQuery<PosicaoFimAnoResult>(
                @$"
                    SELECT
                        ativo.codigo,
                        ativo.cnpj,
                        ativo.nome AS NomeAtivo,
                        posicao_fim_ano.posicao,
                        ROUND(posicao_fim_ano.preco_medio, 2) AS PrecoMedio,
                        ROUND(posicao_fim_ano.custo_total, 2) AS CustoTotal,
                        tipo_ativo.nome AS TipoAtivo
                    FROM
                        posicao_fim_ano
                    LEFT JOIN ativo ON posicao_fim_ano.ativo_id = ativo.id
                    LEFT JOIN tipo_ativo ON ativo.tipo_ativo_id = tipo_ativo.id
                    WHERE ano = {ano}
                    ORDER BY ativo.codigo
                "
            )
            .ToListAsync();
    }
}
