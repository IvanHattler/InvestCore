using InvestCore.Domain.Models;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class InstrumentCalculationModelBase
    {
        public InstrumentClassType ClassType { get; set; }
        public decimal TargetPercent { get; set; }

        public InstrumentType TickerType { get; set; }
        public string Ticker { get; set; }
    }
}
