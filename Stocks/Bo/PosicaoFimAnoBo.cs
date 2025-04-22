using Microsoft.EntityFrameworkCore;
using Stocks.Data;

namespace Stocks.Bo;

public class PosicaoFimAnoResult
{
    public string Codigo { get; set; }
    public string Cnpj { get; set; }
    public string NomeAtivo { get; set; }
    public decimal Posicao { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal CustoTotal { get; set; }
    public string TipoAtivo { get; set; }
}

public class PosicaoFimAnoBo
{
    public async Task<List<PosicaoFimAnoResult>> PosicaoFimAnoQuery(BancoContext db, string ano)
    {
        return await db
            .Database.SqlQuery<PosicaoFimAnoResult>(
                @$"
                    SELECT
                        ativo.codigo,
                        ativo.cnpj,
                        ativo.nome AS nome_ativo,
                        posicao_fim_ano.posicao,
                        ROUND(posicao_fim_ano.preco_medio, 2) AS preco_medio,
                        ROUND(posicao_fim_ano.custo_total, 2) AS custo_total,
                        tipo_ativo.nome AS tipo_ativo
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
