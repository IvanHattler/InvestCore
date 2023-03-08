using InvestCore.Domain.Models;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class TickerInfo : TickerInfoWithCount
    {
        public InstrumentClassType ClassType { get; set; }
    }
}
