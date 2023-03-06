using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvestCore.SpreadsheetsApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InvestCore.SpreadsheetsApi.Services.Implementation
{
    public class SpreadsheetService : ISpreadsheetService
    {
        private readonly SheetsService _sheetsService;
        private readonly ILogger _logger;

        public SpreadsheetService(SheetsService sheetsService, ILogger logger)
        {
            _sheetsService = sheetsService;
            _logger = logger;
        }

        public async Task SendMainTableAsync(List<IList<object>> mainTableData, int startRow, int startColumn, string sheet, string spreadsheetId)
        {
            var valueRange = new ValueRange
            {
                Values = mainTableData
            };
            var endRow = mainTableData.Count + 4;
            var endColumn = startColumn + mainTableData.Max(x => x.Count);
            string range = GetTableRange(sheet, startRow + 1, startColumn + 1, endRow, endColumn);

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var updateResponse = await updateRequest.ExecuteAsync();
            _logger.LogInformation("Updated cells in range: {range}", range);

            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    //Set format to all
                    //GetFormatAllTextRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    //Merge first cell
                    GetMergeCellsRequest(startRow, startColumn, startRow + 1, endColumn, sheetId),
                    //Set border to first cell
                    GetUpdateBordersRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                    //Set border to second cell
                    GetUpdateBordersRequest(startRow + 1, startColumn, startRow + 2, endColumn, sheetId),
                    //Set border to table
                    GetUpdateBordersRequest(startRow + 1, startColumn, endRow - 1, endColumn, sheetId),
                    //Set border to last cells
                    GetUpdateBordersRequest(endRow - 1, startColumn + 2, endRow, endColumn, sheetId),
                    //Set format to first cell
                    GetFormatTextCenterRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                }
            }, spreadsheetId);

            var batchResponce = await batchRequest.ExecuteAsync();
            _logger.LogInformation("Style applied");
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

        private string GetTableRange(string sheet, int startRow, int startColumn, int endRow, int endColumn)
            => $"{sheet}!{GetCellName(startColumn, startRow)}:{GetCellName(endColumn, endRow)}";

        private string GetCellName(int columnIndex, int rowIndex)
            => $"{ToColumnIndex(columnIndex)}{rowIndex}";

        private char ToColumnIndex(int columnIndex)
            => (char)(columnIndex + 0x40);
    }
}
