namespace InvestCore.Domain.Models
{
    public class TickerInfoBase
    {
        public int Count { get; set; }
        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; }
    }
}
