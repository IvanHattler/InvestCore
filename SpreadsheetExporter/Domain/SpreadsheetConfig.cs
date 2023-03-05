using Google.Apis.Sheets.v4;

namespace SpreadsheetExporter.Domain
{
    public class SpreadsheetConfig
    {
        public string[] Scopes => new string[] { SheetsService.Scope.Spreadsheets };
        public string ApplicationName => nameof(SpreadsheetExporter);
        public string SpreadsheetId { get; set; }
        public string Sheet { get; set; }
    }
}
