using InvestCore.PercentCalculateConsole.Services.Interfaces;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class BuyModel
    {
        public int ShareCounts;
        public int GosBondCounts;
        public int CorpBondCounts;
        public decimal SumDifference;
        public decimal SharePercentDeviation;
        public decimal GosBondPercentDeviation;
        public decimal CorpBondPercentDeviation;

        public decimal GetMetric(IMetricStrategy strategy)
            => strategy.GetMetric(this);
    }
}
