﻿using Google.Apis.Sheets.v4;
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

        private const int maxColumn = 100;
        private const int maxRow = 10000;

        public SpreadsheetService(SheetsService sheetsService, ILogger logger)
        {
            _sheetsService = sheetsService;
            _logger = logger;
        }

        public async Task SendMainTableAsync(List<IList<object>> mainTableData, int startRow, int startColumn, string sheet, string spreadsheetId)
        {
            var endRow = mainTableData.Count + 4;
            var endColumn = startColumn + mainTableData.Max(x => x.Count);

            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    GetMoveAllSheetDownRequest(endRow + 2, sheetId),
                    GetSeparatorRequest(endRow + 2, sheetId),

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
                    //Set format to second row
                    GetFormatTextCenterRequest(startRow + 1, startColumn, startRow + 2, endColumn, sheetId),
                    //Set format to all cells
                    GetFormatAllTextRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    //Set percent format to last column
                    GetFormatPercentRequest(startRow + 2, endColumn - 1 , endRow, endColumn, sheetId),

                    GetConditionalFormattingRequest(startRow + 2, endColumn - 1, endRow - 1, endColumn, sheetId),
                }
            }, spreadsheetId);

            await batchRequest.ExecuteAsync();
            _logger.LogInformation("Style applied");

            await SetTableValues(mainTableData, startRow, startColumn, sheet, spreadsheetId, endRow, endColumn);
        }

        private async Task SetTableValues(List<IList<object>> mainTableData, int startRow, int startColumn, string sheet, string spreadsheetId, int endRow, int endColumn)
        {
            var valueRange = new ValueRange
            {
                Values = mainTableData
            };
            string range = SpreadsheetHelper.GetTableRange(sheet, startRow + 1, startColumn + 1, endRow, endColumn);

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            await updateRequest.ExecuteAsync();
            _logger.LogInformation("Updated cells in range: {range}", range);
        }


        private static Request GetConditionalFormattingRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            return new Request()
            {
                AddConditionalFormatRule = new AddConditionalFormatRuleRequest()
                {
                    Rule = new ConditionalFormatRule()
                    {
                        Ranges = new List<GridRange>()
                        {
                            new GridRange()
                            {
                                SheetId = sheetId,
                                StartRowIndex = startRow,
                                StartColumnIndex = startColumn,
                                EndColumnIndex = endColumn,
                                EndRowIndex = endRow,
                            }
                        },
                        GradientRule = new GradientRule()
                        {
                            Minpoint = new InterpolationPoint()
                            {
                                Value = "0",
                                Type = "NUMBER",
                                ColorStyle = new ColorStyle()
                                {
                                    RgbColor = SpreadsheetHelper.Green,
                                }
                            },
                            Midpoint = new InterpolationPoint()
                            {
                                Value = "0,15",
                                Type = "NUMBER",
                                ColorStyle = new ColorStyle()
                                {
                                    RgbColor = SpreadsheetHelper.Yellow,
                                }
                            },
                            Maxpoint = new InterpolationPoint()
                            {
                                Value = "1",
                                Type = "NUMBER",
                                ColorStyle = new ColorStyle()
                                {
                                    RgbColor = SpreadsheetHelper.Red,
                                }
                            }
                        }
                    }
                }
            };
        }


        private static Request GetSeparatorRequest(int rowIndex, int sheetId)
        {
            return new Request()
            {
                UpdateBorders = new UpdateBordersRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = rowIndex,
                        StartColumnIndex = 0,
                        EndRowIndex = rowIndex + 1,
                        EndColumnIndex = maxColumn,
                    },
                    Bottom = SpreadsheetHelper.Border,
                },
            };
        }

        private static Request GetMoveAllSheetDownRequest(int rowCount, int sheetId)
        {
            return new Request()
            {
                CopyPaste = new CopyPasteRequest()
                {
                    Source = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = 0,
                        StartColumnIndex = 0,
                        EndColumnIndex = maxColumn,
                        EndRowIndex = maxRow,
                    },
                    Destination = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = rowCount,
                        StartColumnIndex = 0,
                        EndColumnIndex = maxColumn,
                        EndRowIndex = maxRow + rowCount,
                    },
                    PasteType = "PASTE_NORMAL",
                    PasteOrientation = "NORMAL",
                }
            };
        }

        private static Request GetFormatPercentRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId)
        {
            var cellData = new CellData()
            {
                UserEnteredFormat = new CellFormat()
                {
                    NumberFormat = new NumberFormat
                    {
                        Pattern = "0.00%",
                        Type = "NUMBER"
                    }
                }
            };

            return new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {
                    Fields = "userEnteredFormat.numberFormat",
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = startRow,
                        StartColumnIndex = startColumn,
                        EndColumnIndex = endColumn,
                        EndRowIndex = endRow,
                    },
                    Rows = SpreadsheetHelper.RepeatToList(endRow - startRow, new RowData()
                    {
                        Values = SpreadsheetHelper.RepeatToList(endColumn - startColumn, cellData)
                    }),
                }
            };
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
                    Rows = SpreadsheetHelper.RepeatToList(endRow - startRow, new RowData()
                    {
                        Values = SpreadsheetHelper.RepeatToList(endColumn - startColumn, new CellData()
                        {
                            UserEnteredFormat = new CellFormat()
                            {
                                TextFormat = new TextFormat()
                                {
                                    FontSize = 12,
                                    FontFamily = "Arial",
                                }
                            }
                        })
                    }),
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
                    Rows = SpreadsheetHelper.RepeatToList(endRow - startRow, new RowData()
                    {
                        Values = SpreadsheetHelper.RepeatToList(endColumn - startColumn, new CellData()
                        {
                            UserEnteredFormat = new CellFormat()
                            {
                                HorizontalAlignment = "CENTER",
                            }
                        })
                    })
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
                    Top = SpreadsheetHelper.Border,
                    Bottom = SpreadsheetHelper.Border,
                    Left = SpreadsheetHelper.Border,
                    Right = SpreadsheetHelper.Border,
                    InnerVertical = SpreadsheetHelper.Border,
                },
            };
        }
    }
}
