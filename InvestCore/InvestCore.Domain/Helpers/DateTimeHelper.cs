namespace InvestCore.Domain.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetUTCPlus4DateTime()
        {
            var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
            return utc.AddHours(4);
        }
    }
}
