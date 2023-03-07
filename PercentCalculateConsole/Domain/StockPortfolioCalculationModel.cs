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

        public IEnumerable<(string, decimal)> GetDefaultPricesForBuy()
        {
            if (Share.DefaultPrice.HasValue)
                yield return (Share.Ticker, Share.DefaultPrice.Value);

            if (GosBond.DefaultPrice.HasValue)
                yield return (GosBond.Ticker, GosBond.DefaultPrice.Value);

            if (CorpBond.DefaultPrice.HasValue)
                yield return (CorpBond.Ticker, CorpBond.DefaultPrice.Value);

            if (Gold.DefaultPrice.HasValue)
                yield return (Gold.Ticker, Gold.DefaultPrice.Value);
        }

        public Dictionary<string, decimal> GetDefaultPrices()
            => TickerInfos
                .Where(x => x.DefaultPrice.HasValue)
                .Select(x => (x.Ticker, x.DefaultPrice.Value))
                .Union(GetDefaultPricesForBuy())
                .ToDictionary(x => x.Item1, x => x.Item2);
    }
}
