using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
