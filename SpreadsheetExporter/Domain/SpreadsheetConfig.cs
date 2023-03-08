using Google.Apis.Sheets.v4;

namespace SpreadsheetExporter.Domain
{
    public class SpreadsheetConfig
    {
        public static string[] Scopes => new string[] { SheetsService.Scope.Spreadsheets };
        public string ApplicationName => nameof(SpreadsheetExporter);
        public string SpreadsheetId { get; set; } = string.Empty;
        public string Sheet { get; set; } = string.Empty;
    }
}
