﻿namespace InvestCore.SpreadsheetsApi.Services.Interfaces
{
    public interface ISpreadsheetService
    {
        Task SendMainTableAsync(List<IList<object>> mainTableData, int startRow, int startColumn, string sheet, string spreadsheetId);
    }
}