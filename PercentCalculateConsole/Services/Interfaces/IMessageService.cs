using InvestCore.PercentCalculateConsole.Domain;
using PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IMessageService
    {
        string GetBuyMessage(BuyModel? model);

        string GetOverallMessage(decimal newOverallShares, decimal newOverallGosBonds, decimal newOverallCorpBonds, decimal newOverall);

        string GetOverallMessage(StockPortfolioCalculationModel stockPortfolio);

        string GetResultMessage(StockPortfolioCalculationModel stockPortfolio);

        string GetTestResultMessage(StockPortfolioCalculationModel stockPortfolio);
    }
}
