namespace InvestCore.Domain.Models
{
    public class TickerInfo : TickerInfoWithCount
    {
        public InstrumentClassType ClassType { get; set; }
    }
}
