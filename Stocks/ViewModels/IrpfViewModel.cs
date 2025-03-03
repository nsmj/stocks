using Stocks.BoQueries;

namespace Stocks.ViewModels
{
    public class IrpfViewModel
    {
        public string? AnoFiltrado { get; set; }
        public decimal LucroVendasAbaixo20k { get; set; }
        public Dictionary<int, IrpfRow>? SwingTradeRows { get; set; }
        public Dictionary<int, IrpfRow>? DayTradeRows { get; set; }
        public Dictionary<int, IrpfRow>? FiiRows { get; set; }
    }
}
