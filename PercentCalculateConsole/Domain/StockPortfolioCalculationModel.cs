using InvestCore.Domain.Models;
using PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class StockPortfolioCalculationModel
    {
        public ReplenishmentModel Replenishment { get; set; } = new();
        public InstrumentCalculationModel Share { get; set; } = new();
        public InstrumentCalculationModel GosBond { get; set; } = new();
        public InstrumentCalculationModel CorpBond { get; set; } = new();
        public InstrumentCalculationModel Gold { get; set; } = new();
        public TickerInfo[] TickerInfos { get; set; } = Array.Empty<TickerInfo>();
        public int MonthCountForCalculate { get; set; } = 1;

        public IEnumerable<TickerInfoBase> GetAllTickerInfos()
            => TickerInfos
                .Union(new TickerInfoBase[]
                {
                    Share,
                    GosBond,
                    CorpBond,
                    Gold
                });
    }
}
