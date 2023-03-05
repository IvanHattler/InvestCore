﻿using InvestCore.Domain.Models;

namespace PercentCalculateConsole.Domain
{
    public class InstrumentCalculationModel : TickerInfoBase
    {
        public InstrumentClassType ClassType { get; set; }
        public decimal TargetPercent { get; set; }
        public decimal OverallSum { get; set; }
        public decimal Price { get; set; }
    }
}
