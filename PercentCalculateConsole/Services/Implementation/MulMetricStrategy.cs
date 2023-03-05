using PercentCalculateConsole.Domain;
using PercentCalculateConsole.Services.Interfaces;

namespace PercentCalculateConsole.Services.Implementation
{
    public class MulMetricStrategy : IMetricStrategy
    {
        private const decimal AdditionCoef = 0.0001m;

        public decimal GetMetric(BuyModel model)
            => model.SumDifference
                * (AdditionCoef + model.SharePercentDeviation)
                * (AdditionCoef + model.GosBondPercentDeviation)
                * (AdditionCoef + model.CorpBondPercentDeviation);
    }
}
