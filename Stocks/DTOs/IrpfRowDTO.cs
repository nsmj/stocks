using System.ComponentModel.DataAnnotations;

namespace Stocks.DTOs;

public class IrpfRowDTO
{
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal Total { get; set; }

    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal ImpostoPago { get; set; }

    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal Irrf { get; set; }

    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal PrejuizoAcumulado { get; set; }
}
