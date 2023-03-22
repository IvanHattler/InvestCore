using InvestCore.Domain.Models;
using SpreadsheetExporter.Domain;

namespace SpreadsheetExporter.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task UpdateTableAsync(TickerInfo[] tickerInfos, SpreadsheetConfig spreadsheetConfig,
            ReplenishmentModel replenishment,
            PortfolioInvestmentModel portfolioInvestment);
    }
}
