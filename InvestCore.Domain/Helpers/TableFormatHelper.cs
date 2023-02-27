using System.Text;

namespace InvestCore.Domain.Helpers
{
    public static class TableFormatHelper
    {
        public static string ToMonospaceFontString(string str) => '`' + str + '`';

        public static string GetTable(string[][] data, int minSpacesBetweenColumns, string separator)
        {
            var strCountInOneLine = data.First().Length;

            if (strCountInOneLine <= 0)
            {
                return string.Empty;
            }

            int[] maxLengthOfStrings = GetMaxLengthOfStrings(data, strCountInOneLine);

            List<string> resultLines = GetFormattedLinesForTable(data, minSpacesBetweenColumns, separator, strCountInOneLine, maxLengthOfStrings);

            AddDashLines(resultLines, maxLengthOfStrings, separator);

            return ToMonospaceFontString(ToString(resultLines));
        }

        private static List<string> GetFormattedLinesForTable(string[][] data, int minSpacesBetweenColumns, string separator, int strCountInOneLine, int[] maxLengthOfStrings)
        {
            var resultLines = new List<string>();
            for (int i = 0; i < data.Length; i++)
            {
                resultLines.Add(string.Empty);
                for (int j = strCountInOneLine - 1; j > 0; j--)
                {
                    var last = j == strCountInOneLine - 1
                        ? $"{data[i][j]}{new string(' ', maxLengthOfStrings[j] - data[i][j].Length)}{separator}"
                        : string.Empty;

                    var spaceCount = maxLengthOfStrings[j - 1] + minSpacesBetweenColumns - data[i][j - 1].Length;
                    resultLines[i] = $"{data[i][j - 1]}{new string(' ', spaceCount)}{separator}{last}" + resultLines[i];
                }
            }

            return resultLines;
        }

        private static int[] GetMaxLengthOfStrings(string[][] data, int strCountInOneLine)
        {
            var maxLen = new int[strCountInOneLine];
            for (int i = 0; i < strCountInOneLine; i++)
            {
                maxLen[i] = data.Max(x => x[i].Length);
            }

            return maxLen;
        }

        private static string ToString(IEnumerable<string> resultLines)
        {
            var sb = new StringBuilder();
            foreach (var line in resultLines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private static void AddDashLines(List<string> resultLines, int[] maxLengthOfStrings, string separator)
        {
            string dashLine = GetDashLine(resultLines, maxLengthOfStrings, separator);

            resultLines.Insert(1, dashLine);
            resultLines.Insert(resultLines.Count - 1, dashLine);
        }

        private static string GetDashLine(List<string> resultLines, int[] maxLengthOfStrings, string separator)
        {
            var sb = new StringBuilder(new string('-', resultLines.First().Length));
            var counter = 0;
            for (int i = 0; i < maxLengthOfStrings.Length; i++)
            {
                counter += maxLengthOfStrings[i];
                if (i > 0)
                {
                    counter++;
                }

                sb.Remove(counter, separator.Length);
                sb.Insert(counter, separator);
            }

            var dashLine = sb.ToString();
            return dashLine;
        }

        public static string GetFormattedNumber(decimal price)
        {
            var abs = Math.Abs(price);

            if (abs < 1)
                return price.ToString("F3");

            if (abs < 10)
                return price.ToString("F2");

            if (abs < 1000)
                return price.ToString("F1");

            return abs.ToString("F0");
        }
    }
}
