using InvestCore.Domain.Helpers;
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

        public async Task UpdateTableAsync(TickerInfo[] tickerInfos, SpreadsheetConfig spreadsheetConfig,
            ReplenishmentModel replenishment,
            PortfolioInvestmentModel portfolioInvestment)
        {
            var startRow = 4;
            var startColumn = 1;

            var prices = await _shareService.GetCurrentOrLastPricesAsync(tickerInfos);
            var mainTableData = GetMainTableAsync(tickerInfos, startColumn + 1, startRow + 1, prices);

            var endColumn = startColumn + mainTableData.Max(x => x.Count) + 1;
            var percentOfInstrumentsTable = GetPercentOfInstrumentsTable(tickerInfos, prices);

            var minRowsCount = 22;
            var moveToRow = Math.Max(
                mainTableData.Count + startRow - percentOfInstrumentsTable.Count,
                minRowsCount + startRow - percentOfInstrumentsTable.Count);
            var dictionaryTable = GetDictionaryTable(tickerInfos, endColumn, startRow, prices, replenishment.CurrentSum, portfolioInvestment);

            await _spreadsheetService.SendMainTableAsync(mainTableData, startRow, startColumn, spreadsheetConfig.Sheet,
                spreadsheetConfig.SpreadsheetId, minRowsCount);

            await _spreadsheetService.SendCurrentDate(DateTimeHelper.GetUTCPlus4DateTime(), 3, 1, spreadsheetConfig.Sheet, spreadsheetConfig.SpreadsheetId);
            await _spreadsheetService.SendPercentsOfInsrumentsTable(percentOfInstrumentsTable, moveToRow, endColumn,
                spreadsheetConfig.Sheet, spreadsheetConfig.SpreadsheetId);

            await _spreadsheetService.SendDictionaryTable(dictionaryTable, startRow, endColumn,
                spreadsheetConfig.Sheet, spreadsheetConfig.SpreadsheetId);
        }

        protected List<IList<object>> GetMainTableAsync(TickerInfo[] tickerInfos,
            int firstColumnIndex, int firstRowIndex, Dictionary<string, decimal> prices)
        {
            var result = new List<IList<object>>(tickerInfos.Length + 3)
            {
                new[] { "Общие доли портфеля" },
                new[] { "Название", "Цена, р", "Количество", "Стоимость, р", "Доля" },
            };

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

        protected List<IList<object>> GetPercentOfInstrumentsTable(TickerInfo[] tickerInfos, Dictionary<string, decimal> prices)
            => new List<IList<object>>().WithAction(x =>
            {
                x.Add(new[] { "Доли портфеля по классам активов" });
                var sum = tickerInfos.Sum(y => prices[y.Ticker] * y.Count);
                foreach (var grouping in tickerInfos.GroupBy(x => x.ClassType))
                {
                    x.Add(new object[]
                    {
                        grouping.Key.GetDisplayText(),
                        grouping.Sum(y => prices[y.Ticker] * y.Count) / sum
                    });
                }
            });

        protected List<IList<object>> GetDictionaryTable(TickerInfo[] tickerInfos,
            int firstColumnIndex, int firstRowIndex,
            Dictionary<string, decimal> prices, decimal currentSum,
            PortfolioInvestmentModel portfolioInvestment)
        {
            var usdrub = _shareService.GetUSDRUBAsync().Result;

            var portfolioSum = SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 7);
            var portfolioResultFormula = $"({SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 3)}" +
                    $"+{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 4)}" +
                    $"+{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 5)}" +
                    $"+{portfolioSum}" +
                    $"-{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 6)})";

            var portfolioSumNonBlocked = SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 8);
            var portfolioResultFormulaNonBlocked = $"({SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 3)}" +
                    $"+{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 4)}" +
                    $"+{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 5)}" +
                    $"+{portfolioSumNonBlocked}" +
                    $"-{SpreadsheetHelper.GetCellName(firstColumnIndex + 2, firstRowIndex + 6)})";
            var result = new List<IList<object>>
            {
                new[] { "Справочная информация" },
                new List<object>
                {
                    "Курс доллара, р", $"{usdrub:F2}"
                },
                new List<object>
                {
                    "Денежные средства, р", $"{currentSum:F2}"
                },
                new List<object>
                {
                    "Общие дивиденды и купоны, р", $"{portfolioInvestment.DividendsAndCouponsOverall}"
                },
                new List<object>
                {
                    "Общая выгода по ИИС, р", $"{portfolioInvestment.IISBonusesOverall}"
                },
                new List<object>
                {
                    "Общие внесения, р", $"{portfolioInvestment.DepositsOverall}"
                },
                new List<object>
                {
                    "Стоимость портфеля, р", $"{tickerInfos.Sum(x => prices[x.Ticker] * x.Count) + currentSum}"
                },
                new List<object>
                {
                    "Стоимость портфеля (без замороженных), р", $"{tickerInfos.Where(x => !x.IsBlocked).Sum(x => prices[x.Ticker] * x.Count) + currentSum}"
                },
                new List<object>
                {
                    "Итог портфеля, р", $"=TEXT({portfolioResultFormula};\"0.00\")" +
                        $"&\"  (\"" +
                        $"&TEXT({portfolioResultFormula}/({portfolioSum}-{portfolioResultFormula});\"0.00%\")" +
                        $"&\")\""
                },
                new List<object>
                {
                    "Итог портфеля (без замороженных), р", $"=TEXT({portfolioResultFormulaNonBlocked};\"0.00\")" +
                        $"&\"  (\"" +
                        $"&TEXT({portfolioResultFormulaNonBlocked}/({portfolioSumNonBlocked}-{portfolioResultFormulaNonBlocked});\"0.00%\")" +
                        $"&\")\""
                },
            };

            return result;
        }
    }
}
