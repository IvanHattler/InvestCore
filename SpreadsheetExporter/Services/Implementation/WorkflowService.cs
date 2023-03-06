using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.SpreadsheetsApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
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

        public async Task UpdateAsync(TickerInfoWithCount[] tickerInfos, SpreadsheetConfig spreadsheetConfig)
        {
            var startRow = 4;
            var startColumn = 1;

            var mainTableData = await GetMainTableAsync(tickerInfos, startColumn + 1, startRow + 1);

            await _spreadsheetService.SendMainTableAsync(mainTableData, startRow, startColumn, spreadsheetConfig.Sheet,
                spreadsheetConfig.SpreadsheetId);
        }

        private static Request GetFormatAllTextRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            return new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {
                    Fields = "userEnteredFormat.textFormat",
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
                                        TextFormat = new TextFormat()
                                        {
                                            FontSize = 12,
                                            FontFamily = "Arial",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Request GetFormatTextCenterRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
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
                new[] { "Название", "Цена, р", "Количество", "Стоимость, р", "Доля" },
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
                "Итого:",
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
