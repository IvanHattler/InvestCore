using InvestCore.Domain.Models;
using PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class TickerInfo
    {
        public InstrumentClassType ClassType { get; set; }
        public int Count { get; set; }

        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; }
    }
}
