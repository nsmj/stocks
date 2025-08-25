using Stocks.DTOs;

namespace Stocks.ViewModels
{
    public class IrpfViewModel
    {
        public string? AnoFiltrado { get; set; }
        public decimal LucroVendasAbaixo20k { get; set; }
        public Dictionary<int, IrpfRowDTO>? SwingTradeRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnoAnteriorSwingTrade { get; set; }
        public Dictionary<int, IrpfRowDTO>? DayTradeRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnteriorDayTrade { get; set; }
        public Dictionary<int, IrpfRowDTO>? FiiRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnteriorFii { get; set; }
        public IEnumerable<PosicaoFimAnoViewModel>? PosicoesFimAno { get; set; }
    }
}
