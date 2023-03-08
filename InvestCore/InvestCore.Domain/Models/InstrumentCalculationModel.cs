namespace InvestCore.Domain.Models
{
    public class InstrumentCalculationModel : TickerInfoBase
    {
        public InstrumentClassType ClassType { get; set; }
        public decimal TargetPercent { get; set; }
        public decimal OverallSum { get; set; }
        public decimal Price { get; set; }
        public decimal? DefaultPrice { get; set; }
    }
}
