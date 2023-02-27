using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Interfaces;

namespace InvestCore.PercentCalculateConsole.Services.Implementation
{
    public class StockPortfolioService : IStockPortfolioService
    {
        private IShareService _shareService;

        public StockPortfolioService(IShareService shareService)
        {
            _shareService = shareService ?? throw new ArgumentNullException(nameof(shareService));
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


            //var overallShares = 209748.5m;
            //var overallGosBonds = 18299.3m;
            //var overallCorpBonds = 34043.7m;
            var overallShares = 200200.5m;
            var overallGosBonds = 12159.00m;
            var overallCorpBonds = 30812.70m;
            decimal sharePrice = 11.94m;//tickersForBuyPrices[model.Share.Ticker];
            decimal gosBondPrice = 12.16m;//tickersForBuyPrices[model.GosBond.Ticker];
            decimal corpBondPrice = 12.6m;//1079.8m;//tickersForBuyPrices[model.CorpBond.Ticker];

            model.Share.Price = sharePrice;
            model.Share.OverallSum = overallShares;

            model.GosBond.Price = gosBondPrice;
            model.GosBond.OverallSum = overallGosBonds;

            model.CorpBond.Price = corpBondPrice;
            model.CorpBond.OverallSum = overallCorpBonds;
        }
    }
}
