using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Domain;
using Microsoft.Extensions.Logging;
using PercentCalculateConsole.Domain;
using PercentCalculateConsole.Services.Interfaces;

namespace PercentCalculateConsole.Services.Implementation
{
    public class StockPortfolioService : IStockPortfolioService
    {
        private readonly IShareService _shareService;
        private readonly ILogger _logger;

        public StockPortfolioService(IShareService shareService, ILogger logger)
        {
            _shareService = shareService;
            _logger = logger;
        }

        public void UpdateOverallSum(StockPortfolioCalculationModel stockPortfolio, BuyModel model)
        {
            stockPortfolio.Share.OverallSum += model.ShareCounts * stockPortfolio.Share.Price;
            stockPortfolio.GosBond.OverallSum += model.GosBondCounts * stockPortfolio.GosBond.Price;
            stockPortfolio.CorpBond.OverallSum += model.CorpBondCounts * stockPortfolio.CorpBond.Price;
            stockPortfolio.Gold.OverallSum += model.GoldCounts * stockPortfolio.Gold.Price;
            stockPortfolio.Replenishment.CurrentSum = model.SumDifference;
        }

        public Dictionary<string, decimal> GetStockProfilePrices(StockPortfolioCalculationModel stockPortfolio)
        {
            return _shareService
                .GetCurrentOrLastPricesAsync(stockPortfolio
                    .GetTickersForBuy()
                    .Union(stockPortfolio.TickerInfos
                        .Select(x => (x.Ticker, x.TickerType))))
                .Result;
        }

        public void LoadPricesToModel(StockPortfolioCalculationModel model, IDictionary<string, decimal> prices)
        {
            (model.Share.Price, model.Share.OverallSum) = GetPriceAndOverall(model.Share.Ticker, model.TickerInfos, prices, model.Share.ClassType);
            (model.GosBond.Price, model.GosBond.OverallSum) = GetPriceAndOverall(model.GosBond.Ticker, model.TickerInfos, prices, model.GosBond.ClassType);
            (model.CorpBond.Price, model.CorpBond.OverallSum) = GetPriceAndOverall(model.CorpBond.Ticker, model.TickerInfos, prices, model.CorpBond.ClassType);
            (model.Gold.Price, model.Gold.OverallSum) = GetPriceAndOverall(model.Gold.Ticker, model.TickerInfos, prices, model.Gold.ClassType);
        }

        private static (decimal, decimal) GetPriceAndOverall(string ticker, TickerInfo[] tickerInfos, IDictionary<string, decimal> prices,
            InstrumentClassType classType)
            => (prices[ticker],
                tickerInfos
                    .Where(x => x.ClassType == classType)
                    .Sum(x => prices[x.Ticker] * x.Count));
    }
}
