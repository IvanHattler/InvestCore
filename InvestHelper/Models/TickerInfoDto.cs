using InvestCore.Domain.Models;

namespace InvestHelper.Models
{
    public class TickerInfoDto
    {
        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
    }
}
