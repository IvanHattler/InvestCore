﻿using PercentCalculateConsole.Services.Interfaces;

namespace PercentCalculateConsole.Domain
{
    public class BuyModel
    {
        public int ShareCounts;
        public int GosBondCounts;
        public int CorpBondCounts;
        public int GoldCounts;
        public decimal SumDifference;
        public decimal SharePercentDeviation;
        public decimal GosBondPercentDeviation;
        public decimal CorpBondPercentDeviation;
        public decimal GoldPercentDeviation;

        public decimal GetMetric(IMetricStrategy strategy)
            => strategy.GetMetric(this);
    }
}
