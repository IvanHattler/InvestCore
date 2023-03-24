using System;
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

        private const int maxColumn = 100;
        private const int maxRow = 10000;

        public SpreadsheetService(SheetsService sheetsService, ILogger logger)
        {
            _sheetsService = sheetsService;
            _logger = logger;
        }

        public async Task SendMainTableAsync(List<IList<object>> mainTableData, int startRow, int startColumn, string sheet, string spreadsheetId)
        {
            var endRow = mainTableData.Count + startRow;
            var endColumn = startColumn + mainTableData.Max(x => x.Count);

            var sheetId = 0;

            var getChartsRequest = _sheetsService.Spreadsheets.Get(spreadsheetId);
            var spreadsheet = await getChartsRequest.ExecuteAsync();
            var charts = spreadsheet.Sheets[0].Charts ?? Array.Empty<EmbeddedChart>();
            var moveChartsRequests = GetMoveChartsDownRequest(charts, endRow + 2, sheetId);

            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = moveChartsRequests.Concat(new List<Request>
                {
                    //Move all sheet down
                    GetMoveAllSheetDownRequest(endRow + 2, sheetId),

                    //Print horizontal separator line
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
                    GetFormatHorizontalAlignmentRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                    //Set format to second row
                    GetFormatHorizontalAlignmentRequest(startRow + 1, startColumn, startRow + 2, endColumn, sheetId),
                    //Set format to all cells
                    GetFormatAllTextRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    //Set percent format to last column
                    GetFormatNumberRequest(startRow + 2, endColumn - 1, endRow, endColumn, sheetId),
                    GetFormatNumberRequest(startRow + 2, endColumn - 2, endRow, endColumn - 1, sheetId, pattern: "0.00"),
                    GetFormatNumberRequest(startRow + 2, startColumn + 1, endRow, startColumn + 2, sheetId, pattern: "0.00"),

                    //Add conditional percent formatting
                    GetConditionalFormattingRequest(startRow + 2, endColumn - 1, endRow - 1, endColumn, sheetId),

                    GetAutoResizeDimensionsRequest(0, maxColumn, sheetId),

                    GetPieChartForMainTableRequest(startRow + 1, startColumn, endRow - 1, startColumn + 1, sheetId, startRow, endColumn + 4),
                }).ToList()
            }, spreadsheetId);

            await batchRequest.ExecuteAsync();
            _logger.LogInformation("Main table style applied");

            await SetTableValues(mainTableData, startRow, startColumn, sheet, spreadsheetId, endRow, endColumn);
        }

        public async Task SendCurrentDate(DateTime dateTime, int row, int column, string sheet, string spreadsheetId)
        {
            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    GetFormatAllTextRequest(row - 1, column - 1, row, column, sheetId),
                    GetFormatHorizontalAlignmentRequest(row - 1, column - 1, row, column, sheetId),
                }
            }, spreadsheetId);

            await batchRequest.ExecuteAsync();
            _logger.LogInformation("Current date style applied");

            var valueRange = new ValueRange
            {
                Values = new List<IList<object>>
                {
                    new[] { $"{dateTime:g}" }
                },
            };

            string range = SpreadsheetHelper.GetTableRange(sheet, row, column, row + 1, column + 1);
            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            await updateRequest.ExecuteAsync();
            _logger.LogInformation("Send current date value to range: {range}", range);
        }

        public async Task SendPercentsOfInsrumentsTable(List<IList<object>> table, int startRow, int startColumn, string sheet, string spreadsheetId)
        {
            var endRow = table.Count + startRow;
            var endColumn = startColumn + table.Max(x => x.Count);

            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    GetMergeCellsRequest(startRow, startColumn, startRow + 1, endColumn, sheetId),
                    GetFormatHorizontalAlignmentRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                    GetUpdateBordersRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),

                    //Set format to all cells
                    GetFormatAllTextRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    //Set percent format to last column
                    GetFormatNumberRequest(startRow, endColumn - 1 , endRow, endColumn, sheetId),

                    //Set border to all cells
                    GetUpdateBordersRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    GetPieChartForPercentsTableRequest(startRow + 1, startColumn, endRow, startColumn + 1, sheetId, startRow, endColumn + 1),
                }
            }, spreadsheetId);

            await batchRequest.ExecuteAsync();
            _logger.LogInformation("Percents of instruments table style applied");

            await SetTableValues(table, startRow, startColumn, sheet, spreadsheetId, endRow, endColumn);
        }

        public async Task SendDictionaryTable(List<IList<object>> table, int startRow, int startColumn, string sheet, string spreadsheetId)
        {
            var endRow = table.Count + startRow;
            var endColumn = startColumn + table.Max(x => x.Count);

            var sheetId = 0;
            var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    GetMergeCellsRequest(startRow, startColumn, startRow + 1, endColumn, sheetId),
                    GetFormatHorizontalAlignmentRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),
                    GetUpdateBordersRequest(startRow, startColumn, startRow + 1, startColumn + 1, sheetId),

                    GetFormatHorizontalAlignmentRequest(endRow - 2, endColumn - 1, endRow, endColumn, sheetId, "RIGHT"),
                    GetFormatAllTextRequest(startRow, startColumn, endRow, endColumn, sheetId),
                    GetUpdateBordersRequest(startRow, startColumn, endRow, endColumn, sheetId),
                }
            }, spreadsheetId);

            await batchRequest.ExecuteAsync();
            _logger.LogInformation("Dictionary table style applied");

            await SetTableValues(table, startRow, startColumn, sheet, spreadsheetId, endRow, endColumn);
        }

        #region Private methods

        private static IEnumerable<Request> GetMoveChartsDownRequest(IList<EmbeddedChart> charts, int rowCount, int sheetId)
        {
            foreach (var chart in charts)
            {
                yield return new Request()
                {
                    UpdateEmbeddedObjectPosition = new UpdateEmbeddedObjectPositionRequest()
                    {
                        ObjectId = chart.ChartId,
                        NewPosition = new EmbeddedObjectPosition()
                        {
                            OverlayPosition = new OverlayPosition()
                            {
                                AnchorCell = new GridCoordinate()
                                {
                                    SheetId = chart.Position.OverlayPosition.AnchorCell.SheetId,
                                    RowIndex = chart.Position.OverlayPosition.AnchorCell.RowIndex + rowCount,
                                    ColumnIndex = chart.Position.OverlayPosition.AnchorCell.ColumnIndex,
                                }
                            }
                        },
                        Fields = "anchorCell(rowIndex,columnIndex)"
                    }
                };
            }
        }

        private static Request GetPieChartForMainTableRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId,
            int chartRow, int chartColumn)
        {
            return new Request()
            {
                AddChart = new AddChartRequest()
                {
                    Chart = new EmbeddedChart()
                    {
                        Spec = new ChartSpec()
                        {
                            TitleTextPosition = new TextPosition()
                            {
                                HorizontalAlignment = "CENTER"
                            },
                            AltText = "Доли портфеля",
                            Title = "Доли портфеля",
                            PieChart = new PieChartSpec()
                            {
                                PieHole = 0,
                                LegendPosition = "LABELED_LEGEND",
                                ThreeDimensional = true,
                                Domain = new ChartData()
                                {
                                    SourceRange = new ChartSourceRange()
                                    {
                                        Sources = new List<GridRange>()
                                        {
                                            new GridRange()
                                            {
                                                SheetId = sheetId,
                                                StartRowIndex = startRow,
                                                EndRowIndex = endRow,
                                                StartColumnIndex = startColumn,
                                                EndColumnIndex = endColumn,
                                            }
                                        }
                                    }
                                },
                                Series = new ChartData()
                                {
                                    SourceRange = new ChartSourceRange()
                                    {
                                        Sources = new List<GridRange>()
                                        {
                                            new GridRange()
                                            {
                                                SheetId = sheetId,
                                                StartRowIndex = startRow,
                                                EndRowIndex = endRow,
                                                StartColumnIndex = startColumn + 4,
                                                EndColumnIndex = endColumn + 4,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Position = new EmbeddedObjectPosition()
                        {
                            OverlayPosition = new OverlayPosition()
                            {
                                AnchorCell = new GridCoordinate()
                                {
                                    SheetId = sheetId,
                                    RowIndex = chartRow,
                                    ColumnIndex = chartColumn,
                                },
                                WidthPixels = 500,
                                HeightPixels = 350,
                            }
                        }
                    }
                }
            };
        }

        private static Request GetPieChartForPercentsTableRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId,
            int chartRow, int chartColumn)
        {
            return new Request()
            {
                AddChart = new AddChartRequest()
                {
                    Chart = new EmbeddedChart()
                    {
                        Spec = new ChartSpec()
                        {
                            TitleTextPosition = new TextPosition()
                            {
                                HorizontalAlignment = "CENTER"
                            },
                            AltText = "Доли портфеля по классам активов",
                            Title = "Доли портфеля по классам активов",
                            PieChart = new PieChartSpec()
                            {
                                PieHole = 0,
                                LegendPosition = "LABELED_LEGEND",
                                ThreeDimensional = true,
                                Domain = new ChartData()
                                {
                                    SourceRange = new ChartSourceRange()
                                    {
                                        Sources = new List<GridRange>()
                                        {
                                            new GridRange()
                                            {
                                                SheetId = sheetId,
                                                StartRowIndex = startRow,
                                                EndRowIndex = endRow,
                                                StartColumnIndex = startColumn,
                                                EndColumnIndex = endColumn,
                                            }
                                        }
                                    }
                                },
                                Series = new ChartData()
                                {
                                    SourceRange = new ChartSourceRange()
                                    {
                                        Sources = new List<GridRange>()
                                        {
                                            new GridRange()
                                            {
                                                SheetId = sheetId,
                                                StartRowIndex = startRow,
                                                EndRowIndex = endRow,
                                                StartColumnIndex = startColumn + 1,
                                                EndColumnIndex = endColumn + 1,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Position = new EmbeddedObjectPosition()
                        {
                            OverlayPosition = new OverlayPosition()
                            {
                                AnchorCell = new GridCoordinate()
                                {
                                    SheetId = sheetId,
                                    RowIndex = chartRow,
                                    ColumnIndex = chartColumn,
                                },
                                WidthPixels = 350,
                                HeightPixels = 170,
                            }
                        }
                    }
                }
            };
        }

        private static Request GetAutoResizeDimensionsRequest(int startColumn, int endColumn, int sheetId)
        {
            return new Request()
            {
                AutoResizeDimensions = new AutoResizeDimensionsRequest()
                {
                    Dimensions = new DimensionRange()
                    {
                        SheetId = sheetId,
                        Dimension = "COLUMNS",
                        StartIndex = startColumn,
                        EndIndex = endColumn,
                    }
                }
            };
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
            _logger.LogInformation("Send table values to range: {range}", range);
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
                CutPaste = new CutPasteRequest()
                {
                    Source = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = 0,
                        StartColumnIndex = 0,
                        EndColumnIndex = maxColumn,
                        EndRowIndex = maxRow,
                    },
                    Destination = new GridCoordinate()
                    {
                        SheetId = sheetId,
                        RowIndex = rowCount,
                        ColumnIndex = 0,
                    },
                    PasteType = "PASTE_NORMAL",
                }
            };
        }

        private static Request GetFormatNumberRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId, string pattern = "0.00%")
        {
            var cellData = new CellData()
            {
                UserEnteredFormat = new CellFormat()
                {
                    NumberFormat = new NumberFormat
                    {
                        Pattern = pattern,
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

        private static Request GetFormatHorizontalAlignmentRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId,
            string horizontalAlignment = "CENTER")
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
                                HorizontalAlignment = horizontalAlignment,
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

        private static Request GetUpdateBordersRequest(int startRow, int startColumn, int endRow, int endColumn, int sheetId, bool needHorizontalLines = false)
        {
            var request = new Request()
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

            if (needHorizontalLines)
            {
                request.UpdateBorders.InnerHorizontal = SpreadsheetHelper.Border;
            }

            return request;
        }

        #endregion Private methods
    }
}
