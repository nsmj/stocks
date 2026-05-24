using Stocks.Models;

namespace Stocks.DTOs;

public record DadosNotaNegociacaoDto(List<Operacao> Operacoes, List<Irrf> Irrfs);
