using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;

namespace SpreadsheetExporter.Services.Implementation
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IShareService _shareService;
        private readonly SheetsService _sheetsService;

        public WorkflowService(IShareService shareService, SheetsService sheetsService)
        {
            _shareService = shareService;
            _sheetsService = sheetsService;
        }

        public async Task UpdateAsync(TickerInfoWithCount[] tickerInfos, SpreadsheetConfig spreadsheetConfig)
        {
            var startRow = 5;
            var startColumn = 2;

            var mainTableData = await GetMainTableAsync(tickerInfos, startColumn, startRow);
            var valueRange = new ValueRange();
            valueRange.Values = mainTableData;
            var endRow = mainTableData.Count + 5;
            var endColumn = startColumn + mainTableData.Max(x => x.Count) - 1;
            var range = $"{spreadsheetConfig.Sheet}!{GetCellName(startColumn, startRow)}:{GetCellName(endColumn, endRow)}";

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetConfig.SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var updateResponse = updateRequest.Execute();

            var batchRequest = _sheetsService.Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest(), spreadsheetConfig.SpreadsheetId);
        }

        public async Task<List<IList<object>>> GetMainTableAsync(TickerInfoWithCount[] tickerInfos,
            int firstColumnIndex, int firstRowIndex)
        {
            var result = new List<IList<object>>(tickerInfos.Length + 3)
            {
                new[] { "Общие доли портфеля" },
                new[] { "Название", "Цена", "Количество", "Стоимость, р", "Доля" },
            };

            var prices = await _shareService.GetCurrentOrLastPricesAsync(
                tickerInfos.Select(x => (x.Ticker, x.TickerType)));

            var currentRowIndex = firstRowIndex + 1;
            var len = tickerInfos.Length + 2;
            var sumCellName = GetCellName(firstColumnIndex + 3, len + firstRowIndex);
            foreach (var tickerInfo in tickerInfos)
            {
                currentRowIndex++;

                result.Add(new object[]
                {
                    tickerInfo.Ticker,
                    prices[tickerInfo.Ticker],
                    tickerInfo.Count.ToString(),
                    $"={GetCellName(firstColumnIndex + 1,currentRowIndex)}*{GetCellName(firstColumnIndex + 2, currentRowIndex)}",
                    $"={GetCellName(firstColumnIndex + 3, currentRowIndex)}/{sumCellName}"
                });
            }

            result.Add(new string[]
            {
                "",
                "",
                "",
                $"=SUM({GetCellName(firstColumnIndex + 3, firstRowIndex + 2)}:{GetCellName(firstColumnIndex + 3, currentRowIndex)})",
                $"=SUM({GetCellName(firstColumnIndex + 4, firstRowIndex + 2)}:{GetCellName(firstColumnIndex + 4, currentRowIndex)})"
            });

            return result;
        }

        private string GetCellName(int columnIndex, int rowIndex)
            => $"{ToColumnIndex(columnIndex)}{rowIndex}";

        private char ToColumnIndex(int columnIndex)
            => (char)(columnIndex + 0x40);
    }
}
