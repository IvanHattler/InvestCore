using InvestCore.PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IMessageService
    {
        string GetResultMessage(StockPortfolioCalculationModel stockPortfolio);

        string GetTestResultMessage(StockPortfolioCalculationModel stockPortfolio);
    }
}
