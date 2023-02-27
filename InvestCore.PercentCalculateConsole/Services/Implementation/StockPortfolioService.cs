using System.Text.Json;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class StockPortfolioService : IStockPortfolioService
    {
        private IShareService _shareService;
        private ILogger _logger;

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
        }

        public void LoadPricesToModel(StockPortfolioCalculationModel model)
        {
            //var tickersForBuyPrices = _shareService
            //    .GetPricesAsync(model.GetTickers())
            //    .Result;

            //var stockProfilePrices = _shareService
            //    .GetPricesAsync(
            //        model.TickerInfos.Select(x => (x.Ticker, x.TickerType)))
            //    .Result;

            //var overallShares = model.TickerInfos
            //    .Where(x => x.ClassType == InstrumentClassType.Share)
            //    .Sum(x => stockProfilePrices[x.Ticker]);

            //var overallGosBonds = model.TickerInfos
            //    .Where(x => x.ClassType == InstrumentClassType.GosBond)
            //    .Sum(x => stockProfilePrices[x.Ticker]);

            //var overallCorpBonds = model.TickerInfos
            //    .Where(x => x.ClassType == InstrumentClassType.CorpBond)
            //    .Sum(x => stockProfilePrices[x.Ticker]);


            var overallShares = 209748.5m;
            var overallGosBonds = 18299.3m;
            var overallCorpBonds = 34043.7m;

            model.Share.Price = 11.94m;//tickersForBuyPrices[model.Share.Ticker];
            model.Share.OverallSum = overallShares;

            model.GosBond.Price = 12.16m;//tickersForBuyPrices[model.GosBond.Ticker];
            model.GosBond.OverallSum = overallGosBonds;

            model.CorpBond.Price = 12.6m;//1079.8m;//tickersForBuyPrices[model.CorpBond.Ticker];
            model.CorpBond.OverallSum = overallCorpBonds;

            _logger.LogInformation(JsonSerializer.Serialize(model.Share));
            _logger.LogInformation(JsonSerializer.Serialize(model.GosBond));
            _logger.LogInformation(JsonSerializer.Serialize(model.CorpBond));
        }
    }
}
