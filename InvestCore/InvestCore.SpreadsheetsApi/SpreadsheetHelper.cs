using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static Color White = new()
        {
            Red = 1,
            Green = 1,
            Blue = 1,
            Alpha = 1
        };

        public static Color Black = new()
        {
            Red = 0,
            Green = 0,
            Blue = 0,
            Alpha = 1
        };

        public static Color Green = new()
        {
            Red = 87f / 255,
            Green = 187f / 255,
            Blue = 138f / 255,
            Alpha = 1
        };

        public static Color Yellow = new()
        {
            Red = 1,
            Green = 1,
            Blue = 0,
            Alpha = 1
        };

        public static Color Red = new()
        {
            Red = 1,
            Green = 0,
            Blue = 0,
            Alpha = 1
        };

        public static Border Border = new()
        {
            Color = Black,
            Style = "SOLID",
            Width = 1
        };
    }
}
