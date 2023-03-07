using InvestCore.Domain.Models;
using PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class TickerInfo : TickerInfoWithCount
    {
        public InstrumentClassType ClassType { get; set; }
        public decimal? DefaultPrice { get; set; }
    }
}
