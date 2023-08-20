using InvestCore.PercentCalculateConsole.Domain;

namespace PercentCalculateConsole.Services.Interfaces
{
    public interface IMessageService
    {
        Task<string> GetResultMessage(StockPortfolioCalculationModel stockPortfolio);

        Task<string> GetTestResultMessage(StockPortfolioCalculationModel stockPortfolio);
    }
}
