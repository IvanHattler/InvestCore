using InvestCore.Domain.Models;
using SpreadsheetExporter.Domain;

namespace SpreadsheetExporter.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task UpdateAsync(TickerInfoWithCount[] tickerInfos, SpreadsheetConfig spreadsheetConfig);
    }
}