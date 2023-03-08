using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.SpreadsheetsApi;
using InvestCore.SpreadsheetsApi.Services.Interfaces;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SpreadsheetExporter.Services.Implementation
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IShareService _shareService;
        private readonly ISpreadsheetService _spreadsheetService;
        private readonly ILogger _logger;

        public WorkflowService(IShareService shareService, ISpreadsheetService spreadsheetService, ILogger logger)
        {
            _shareService = shareService;
            _spreadsheetService = spreadsheetService;
            _logger = logger;
        }

        public async Task UpdateTableAsync(TickerInfoWithCount[] tickerInfos, SpreadsheetConfig spreadsheetConfig)
        {
            var startRow = 4;
            var startColumn = 1;

            var mainTableData = await GetMainTableAsync(tickerInfos, startColumn + 1, startRow + 1);

            await _spreadsheetService.SendMainTableAsync(mainTableData, startRow, startColumn, spreadsheetConfig.Sheet,
                spreadsheetConfig.SpreadsheetId);

            await _spreadsheetService.SendCurrentDate(DateTime.Now, 3, 1, spreadsheetConfig.Sheet, spreadsheetConfig.SpreadsheetId);
        }

        public async Task<List<IList<object>>> GetMainTableAsync(TickerInfoWithCount[] tickerInfos,
            int firstColumnIndex, int firstRowIndex)
        {
            var result = new List<IList<object>>(tickerInfos.Length + 3)
            {
                new[] { "Общие доли портфеля" },
                new[] { "Название", "Цена, р", "Количество", "Стоимость, р", "Доля" },
            };

            var prices = await _shareService.GetCurrentOrLastPricesAsync(tickerInfos);

            var currentRowIndex = firstRowIndex + 1;
            var len = tickerInfos.Length + 2;
            var sumCellName = SpreadsheetHelper.GetCellName(firstColumnIndex + 3, len + firstRowIndex);
            foreach (var tickerInfo in tickerInfos)
            {
                currentRowIndex++;

                result.Add(new object[]
                {
                    tickerInfo.Ticker,
                    prices[tickerInfo.Ticker],
                    tickerInfo.Count.ToString(),
                    $"={SpreadsheetHelper.GetCellName(firstColumnIndex + 1,currentRowIndex)}*{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, currentRowIndex)}",
                    $"={SpreadsheetHelper.GetCellName(firstColumnIndex + 3, currentRowIndex)}/{sumCellName}"
                });
            }

            result.Add(new string[]
            {
                "",
                "",
                "Итого:",
                $"=SUM({SpreadsheetHelper.GetCellName(firstColumnIndex + 3, firstRowIndex + 2)}:{SpreadsheetHelper.GetCellName(firstColumnIndex + 3, currentRowIndex)})",
                $"=SUM({SpreadsheetHelper.GetCellName(firstColumnIndex + 4, firstRowIndex + 2)}:{SpreadsheetHelper.GetCellName(firstColumnIndex + 4, currentRowIndex)})"
            });

            return result;
        }
    }
}
