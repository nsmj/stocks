using Microsoft.EntityFrameworkCore;
using Stocks.Data;
using Stocks.DTOs;
using Stocks.Interfaces;
using Stocks.Models;

namespace Stocks.Extraction;

public class CalcularResultadosUseCase(BancoContext db) : ICalcularResultadosUseCase
{
    public async Task ExecutarAsync()
    {
        var primeiroAnoTransacoes = await db.Operacoes.MinAsync(op => op.Data.Year);

        int anoFinal = DateTime.Now.Year;

        // Busca uma lista dos ativos que possuem operações
        var listaAtivos = await db.Operacoes.Select(op => op.AtivoId).Distinct().ToListAsync();

        List<Task> tasks = [];

        for (int anoAtual = primeiroAnoTransacoes; anoAtual <= anoFinal; anoAtual++)
        {
            foreach (var ativoId in listaAtivos)
            {
                tasks.Add(CalcularResultadosAtivoAnoAsync(anoAtual, ativoId));
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task CalcularResultadosAtivoAnoAsync(int ano, int ativoId)
    {
        var eventosOperacoes = BuscarEventosEOperacoes(ano, ativoId);
        var posicao = await ObterPosicaoInicialAsync(ano, ativoId);

        bool houveMovimentacao = false;
        await foreach (var item in eventosOperacoes.AsAsyncEnumerable())
        {
            houveMovimentacao = true;
            if (item.Tipo == "Operacao")
            {
                await ProcessarOperacaoAsync(item, posicao);
            }
            else
            {
                ProcessarEvento(item, posicao);
            }
        }

        if (houveMovimentacao)
        {
            await RegistrarPosicaoFimAnoAsync(ano, ativoId, posicao);
            await AtualizarAtivoAsync(ativoId, posicao);
        }
        else
        {
            await ManterPosicaoAnteriorAsync(ano, ativoId);
        }

        await db.SaveChangesAsync();
    }

    private IQueryable<DadosCalculoResultadoDTO> BuscarEventosEOperacoes(int ano, int ativoId)
    {
        var anoInicio = $"{ano}-01-01";
        var anoFim = $"{ano}-12-31";

        return db.Database.SqlQuery<DadosCalculoResultadoDTO>(
            @$"
                        SELECT
                            o.id AS Id,
                            'Operacao' AS Tipo,
                            data AS Data,
                            null AS Fator,
                            quantidade AS Quantidade,
                            preco_ativo AS PrecoAtivo,
                            valor_total AS ValorTotal,
                            compra AS Compra,
                            taxas AS Taxas,
                            a.id AS AtivoId,
                            top.nome AS 'TipoOperacaoEvento'
                        FROM operacao o
                        LEFT JOIN ativo a ON o.ativo_id = a.id
                        LEFT JOIN tipo_operacao top ON o.tipo_operacao_id = top.id
                        WHERE o.ativo_id = {ativoId} AND o.data BETWEEN {anoInicio} AND {anoFim}
                        UNION ALL
                        SELECT
                            e.id AS Id,
                            'Evento' AS Tipo,
                            data AS Data,
                            fator AS Fator,
                            null AS Quantidade,
                            Valor AS PrecoAtivo,
                            null AS ValorTotal,
                            null AS Compra,
                            null AS Taxas,
                            a.id AS AtivoId,
                            te.nome
                        FROM evento e
                        LEFT JOIN ativo a ON e.ativo_id = a.id
                        LEFT JOIN tipo_evento te ON e.tipo_evento_id = te.id
                        WHERE e.ativo_id = {ativoId} AND e.data BETWEEN {anoInicio} AND {anoFim}
                        ORDER BY AtivoId, Data
                    "
        );
    }

    private async Task<PosicaoEstado> ObterPosicaoInicialAsync(int ano, int ativoId)
    {
        var ultimaPosicaoFimAno = await db
            .PosicoesFimAno.Where(p => p.AtivoId == ativoId && p.Ano == ano - 1)
            .FirstOrDefaultAsync();

        return new PosicaoEstado(
            ultimaPosicaoFimAno?.Posicao ?? 0,
            ultimaPosicaoFimAno?.PrecoMedio ?? 0
        );
    }

    private async Task ProcessarOperacaoAsync(DadosCalculoResultadoDTO item, PosicaoEstado posicao)
    {
        var taxas = item.Taxas.GetValueOrDefault();
        var quantidade = item.Quantidade.GetValueOrDefault();
        var valorTotal = Convert.ToDecimal(item.ValorTotal.GetValueOrDefault());

        if (item.Compra == 1)
        {
            posicao.RegistrarCompra(quantidade, valorTotal, taxas);
        }
        else
        {
            var lucroLiquido = posicao.RegistrarVenda(quantidade, valorTotal, taxas);
            var operacao = await db.Operacoes.FindAsync(item.Id);
            if (operacao is not null)
            {
                operacao.LucroLiquido = lucroLiquido;
            }
        }
    }

    private static void ProcessarEvento(DadosCalculoResultadoDTO item, PosicaoEstado posicao)
    {
        var fator = item.Fator.GetValueOrDefault();
        var precoAtivo = item.PrecoAtivo.GetValueOrDefault();

        switch (item.TipoOperacaoEvento)
        {
            case "Grupamento":
                posicao.AplicarGrupamento(fator);
                break;
            case "Desdobramento":
                posicao.AplicarDesdobramento(fator);
                break;
            case "Bonificação":
                posicao.AplicarBonificacao(fator, precoAtivo);
                break;
        }
    }

    private async Task RegistrarPosicaoFimAnoAsync(int ano, int ativoId, PosicaoEstado posicao)
    {
        if (posicao.Quantidade <= 0)
            return;

        var posicaoFimAno = new PosicaoFimAno
        {
            AtivoId = ativoId,
            Ano = ano,
            Posicao = posicao.Quantidade,
            PrecoMedio = posicao.PrecoMedio,
            CustoTotal = posicao.CustoTotal,
        };

        db.PosicoesFimAno.Add(posicaoFimAno);
    }

    private async Task AtualizarAtivoAsync(int ativoId, PosicaoEstado posicao)
    {
        var ativo = await db.Ativos.FindAsync(ativoId);
        if (ativo is not null)
        {
            ativo.Posicao = posicao.Quantidade;
            ativo.PrecoMedio = posicao.PrecoMedio;
            db.Ativos.Update(ativo);
        }
    }

    private async Task ManterPosicaoAnteriorAsync(int ano, int ativoId)
    {
        var anterior = await db
            .PosicoesFimAno.Where(p => p.AtivoId == ativoId && p.Ano == ano - 1)
            .FirstOrDefaultAsync();

        if (anterior is { Posicao: > 0 })
        {
            db.PosicoesFimAno.Add(
                new PosicaoFimAno
                {
                    AtivoId = ativoId,
                    Ano = ano,
                    Posicao = anterior.Posicao,
                    PrecoMedio = anterior.PrecoMedio,
                    CustoTotal = anterior.CustoTotal,
                }
            );
        }
    }

    private class PosicaoEstado(int quantidade, decimal precoMedio)
    {
        public int Quantidade { get; private set; } = quantidade;
        public decimal PrecoMedio { get; private set; } = precoMedio;
        public decimal CustoTotal => Quantidade * PrecoMedio;

        public void RegistrarCompra(int qtd, decimal valorTotal, decimal taxas)
        {
            var novaQuantidade = Quantidade + qtd;
            if (novaQuantidade > 0)
            {
                PrecoMedio = (CustoTotal + valorTotal + taxas) / novaQuantidade;
            }
            Quantidade = novaQuantidade;
        }

        public decimal RegistrarVenda(int qtd, decimal valorTotal, decimal taxas)
        {
            var precoMedioVenda = (valorTotal - taxas) / qtd;
            var lucroLiquido = Math.Round((precoMedioVenda - PrecoMedio) * qtd, 2);
            Quantidade -= qtd;
            return lucroLiquido;
        }

        public void AplicarGrupamento(int fator)
        {
            if (fator == 0)
                return;
            Quantidade /= fator;
            PrecoMedio *= fator;
        }

        public void AplicarDesdobramento(int fator)
        {
            Quantidade *= fator;
            if (fator != 0)
                PrecoMedio /= fator;
        }

        public void AplicarBonificacao(int fator, decimal precoAtivo)
        {
            // Converte para int antes para garantir que o custo e a quantidade usem o mesmo valor
            int acoesBonificadas = (int)(Quantidade * (decimal)fator / 100);
            var novaQuantidade = Quantidade + acoesBonificadas;

            if (novaQuantidade > 0)
            {
                PrecoMedio = (CustoTotal + (precoAtivo * acoesBonificadas)) / novaQuantidade;
            }
            Quantidade = novaQuantidade;
        }
    }
}
