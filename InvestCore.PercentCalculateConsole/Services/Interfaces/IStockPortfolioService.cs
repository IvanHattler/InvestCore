using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface IStockPortfolioService
    {
        void UpdateOverallSum(StockPortfolioCalculationModel stockPortfolio, BuyModel model);
        Dictionary<string, decimal> GetStockProfilePrices(StockPortfolioCalculationModel stockPortfolio);
        void LoadPricesToModel(StockPortfolioCalculationModel model, IDictionary<string, decimal> prices);

    }
}
