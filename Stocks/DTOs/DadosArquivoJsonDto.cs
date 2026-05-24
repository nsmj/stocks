using Stocks.Models;

namespace Stocks.DTOs;

public record DadosArquivoJsonDto(List<Operacao> Operacoes, List<Evento> Eventos);
