using InvestCore.PercentCalculateConsole.Domain;
using PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IStockPortfolioService
    {
        void UpdateOverallSum(StockPortfolioCalculationModel stockPortfolio, BuyModel model);
        Dictionary<string, decimal> GetStockProfilePrices(StockPortfolioCalculationModel stockPortfolio);
        void LoadPricesToModel(StockPortfolioCalculationModel model, IDictionary<string, decimal> prices);

    }
}
