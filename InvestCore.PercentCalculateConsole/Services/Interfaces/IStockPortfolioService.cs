using InvestCore.PercentCalculateConsole.Domain;

namespace InvestCore.PercentCalculateConsole.Services.Interfaces
{
    public interface IStockPortfolioService
    {
        void LoadPricesToModel(StockPortfolioCalculationModel model);

        void UpdateOverallSum(StockPortfolioCalculationModel stockPortfolio, BuyModel model);
    }
}
