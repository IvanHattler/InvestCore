namespace InvestCore.Domain.Models
{
    public class UserInfo
    {
        public long UserId { get; set; }

        public int? DataMessageId { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsSubscribed { get; set; }

        public IEnumerable<TickerInfoModel> TickerInfos { get; set; }

        public UserInfo(long userId, bool isAdmin, IEnumerable<TickerInfoModel> tickerInfos)
        {
            UserId = userId;
            IsAdmin = isAdmin;
            TickerInfos = tickerInfos ?? throw new ArgumentNullException(nameof(tickerInfos));
        }
    }
}
