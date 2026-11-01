using System.ComponentModel.DataAnnotations;
using Stocks.DTOs;

namespace Stocks.ViewModels
{
    public class IrpfViewModel
    {
        public string? AnoFiltrado { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LucroVendasAbaixo20k { get; set; }
        public Dictionary<int, IrpfRowDTO>? SwingTradeRows { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? PrejuizoAcumuladoAnoAnoAnteriorSwingTrade { get; set; }
        public Dictionary<int, IrpfRowDTO>? DayTradeRows { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? PrejuizoAcumuladoAnoAnteriorDayTrade { get; set; }
        public Dictionary<int, IrpfRowDTO>? FiiRows { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? PrejuizoAcumuladoAnoAnteriorFii { get; set; }
        public IEnumerable<PosicaoFimAnoViewModel>? PosicoesFimAno { get; set; }
    }
}
