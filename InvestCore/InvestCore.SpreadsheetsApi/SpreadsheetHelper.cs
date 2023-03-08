using Google.Apis.Sheets.v4.Data;

namespace InvestCore.SpreadsheetsApi
{
    public static class SpreadsheetHelper
    {
        public static T WithAction<T>(this T el, Action<T> action)
        {
            action(el);
            return el;
        }

        public static List<T> RepeatToList<T>(int count, T element)
            => new List<T>().WithAction(x =>
                {
                    for (int i = 0; i < count; i++)
                        x.Add(element);
                });

        public static string GetTableRange(string sheet, int startRow, int startColumn, int endRow, int endColumn)
            => $"{sheet}!{GetCellName(startColumn, startRow)}:{GetCellName(endColumn, endRow)}";

        public static string GetCellName(int columnIndex, int rowIndex)
            => $"{ToColumnIndex(columnIndex)}{rowIndex}";

        public static char ToColumnIndex(int columnIndex)
            => (char)(columnIndex + 0x40);

        private static readonly Color white = new()
        {
            Red = 1,
            Green = 1,
            Blue = 1,
            Alpha = 1
        };

        private static readonly Color black = new()
        {
            Red = 0,
            Green = 0,
            Blue = 0,
            Alpha = 1
        };

        private static readonly Color green = new()
        {
            Red = 87f / 255,
            Green = 187f / 255,
            Blue = 138f / 255,
            Alpha = 1
        };

        private static readonly Color yellow = new()
        {
            Red = 1,
            Green = 1,
            Blue = 0,
            Alpha = 1
        };

        private static readonly Color red = new()
        {
            Red = 1,
            Green = 0,
            Blue = 0,
            Alpha = 1
        };

        private static readonly Border border = new()
        {
            Color = Black,
            Style = "SOLID",
            Width = 1
        };

        public static Color Yellow { get => yellow; }
        public static Color Green { get => green; }
        public static Color Red { get => red; }
        public static Border Border { get => border; }
        public static Color Black { get => black; }
        public static Color White { get => white; }
    }
}
