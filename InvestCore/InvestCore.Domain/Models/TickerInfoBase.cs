namespace InvestCore.Domain.Models
{
    public class TickerInfoBase
    {
        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; }
        public decimal? DefaultPrice { get; set; }
    }
}
