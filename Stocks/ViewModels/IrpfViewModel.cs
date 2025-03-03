using Stocks.BoQueries;

namespace Stocks.ViewModels
{
    public class IrpfViewModel
    {
        public string? AnoFiltrado { get; set; }
        public decimal LucroVendasAbaixo20k { get; set; }
        public Dictionary<string, IrpfRow>? SwingTradeRows { get; set; }
    }
}
