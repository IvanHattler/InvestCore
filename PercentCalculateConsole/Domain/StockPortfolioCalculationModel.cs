using InvestCore.Domain.Models;
using PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Domain
{
    public class StockPortfolioCalculationModel
    {
        public ReplenishmentModel Replenishment { get; set; }
        public InstrumentCalculationModel Share { get; set; }
        public InstrumentCalculationModel GosBond { get; set; }
        public InstrumentCalculationModel CorpBond { get; set; }
        public InstrumentCalculationModel Gold { get; set; }
        public TickerInfo[] TickerInfos { get; set; }

        public (string, InstrumentType)[] GetTickersForBuy()
            => new[]
            {
                (Share.Ticker, Share.TickerType),
                (GosBond.Ticker, GosBond.TickerType),
                (CorpBond.Ticker, CorpBond.TickerType),
                (Gold.Ticker, Gold.TickerType),
            };
    }
}
