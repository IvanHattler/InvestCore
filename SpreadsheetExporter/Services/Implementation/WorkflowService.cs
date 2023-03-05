using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SpreadsheetExporter.Services.Implementation
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IShareService _shareService;
        private readonly SheetsService _sheetsService;
        private readonly ILogger _logger;

        public WorkflowService(IShareService shareService, SheetsService sheetsService, ILogger logger)
        {
            _shareService = shareService;
            _sheetsService = sheetsService;
            _logger = logger;
        }

        public async Task UpdateAsync(TickerInfoWithCount[] tickerInfos, SpreadsheetConfig spreadsheetConfig)
        {
            var startRow = 4;
            var startColumn = 1;

            var mainTableData = await GetMainTableAsync(tickerInfos, startColumn + 1, startRow + 1);
            var valueRange = new ValueRange();
            valueRange.Values = mainTableData;
            var endRow = mainTableData.Count + 4;
            var endColumn = startColumn + mainTableData.Max(x => x.Count);
            string range = GetTableRange(spreadsheetConfig, startRow + 1, startColumn + 1, endRow, endColumn);

            _logger.LogInformation("Updated cells in range: {range}", range);

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetConfig.SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var updateResponse = updateRequest.Execute();

            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    //Merge first cell
                    GetMergeCellsRequest(startRow, startColumn, startRow + 1, endColumn, sheetId),
                    //Set border to first cell
                    GetUpdateBordersRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                    //Set border to table
                    GetUpdateBordersRequest(startRow + 1, startColumn, endRow - 1, endColumn, sheetId),
                    //Set border to last cells
                    GetUpdateBordersRequest(endRow - 1, startColumn + 3, endRow, endColumn, sheetId),
                    //SetFormat to first cell
                    GetFormatRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                }
            }, spreadsheetConfig.SpreadsheetId);

            _logger.LogInformation("Style applied");

            var batchResponce = batchRequest.Execute();
        }

        private static Request GetFormatRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            return new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {
                    Fields = "userEnteredFormat.horizontalAlignment",

                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = startRow,
                        StartColumnIndex = startColumn,
                        EndColumnIndex = endColumn,
                        EndRowIndex = endRow,
                    },
                    Rows = new List<RowData>()
                    {
                        new RowData()
                        {
                            Values = new List<CellData>()
                            {
                                new CellData()
                                {
                                    UserEnteredFormat = new CellFormat()
                                    {
                                        HorizontalAlignment = "CENTER",
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Request GetMergeCellsRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            return new Request()
            {
                MergeCells = new MergeCellsRequest()
                {
                    MergeType = "MERGE_ALL",
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = startRow,
                        StartColumnIndex = startColumn,
                        EndRowIndex = endRow,
                        EndColumnIndex = endColumn
                    }
                }
            };
        }

        private static Request GetUpdateBordersRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            var blackColor = new Color()
            {
                Red = 0,
                Green = 0,
                Blue = 0,
                Alpha = 1
            };
            var border = new Border()
            {
                Color = blackColor,
                Style = "SOLID",
                Width = 1
            };
            return new Request()
            {
                UpdateBorders = new UpdateBordersRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = startRow,
                        StartColumnIndex = startColumn,
                        EndRowIndex = endRow,
                        EndColumnIndex = endColumn
                    },
                    Top = border,
                    Bottom = border,
                    Left = border,
                    Right = border,
                    InnerVertical = border,
                },
            };
        }

        private string GetTableRange(SpreadsheetConfig spreadsheetConfig, int startRow, int startColumn, int endRow, int endColumn)
            => $"{spreadsheetConfig.Sheet}!{GetCellName(startColumn, startRow)}:{GetCellName(endColumn, endRow)}";

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
