namespace InvestCore.Domain.Models
{
    public class TickerInfoBase
    {
        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public decimal? DefaultPrice { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
    }
}
