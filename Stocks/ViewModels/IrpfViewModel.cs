using Stocks.BoQueries;

namespace Stocks.ViewModels
{
    public class IrpfViewModel
    {
        public string? AnoFiltrado { get; set; }
        public decimal LucroVendasAbaixo20k { get; set; }
        public Dictionary<int, IrpfRowBo>? SwingTradeRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnoAnteiorSwingTrade { get; set; }
        public Dictionary<int, IrpfRowBo>? DayTradeRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnteriorDayTrade { get; set; }
        public Dictionary<int, IrpfRowBo>? FiiRows { get; set; }
        public decimal? PrejuizoAcumuladoAnoAnteriorFii { get; set; }
    }
}
