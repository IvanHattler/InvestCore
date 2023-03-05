namespace ChatBot.Shares.Integration.Services
{
    //public class ShareTwelveDataService : IMessageService
    //{
    //    private readonly string _token;
    //
    //    public ShareTwelveDataService(string token)
    //    {
    //        _token = token ?? throw new ArgumentNullException(nameof(token));
    //    }
    //
    //    public async Task<string> GetMessageAsync(IEnumerable<TickerInfoModel> tickerInfos)
    //    {
    //        var _client = new HttpClient();
    //        ITwelveDataClient _twelveDataClient = new TwelveDataClient(_token, _client);
    //        var sb = new StringBuilder();
    //
    //        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
    //        sb.AppendLine($"Prices for {DateTime.Now}");
    //        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
    //
    //        int i = 0;
    //        foreach (var info in tickerInfos)
    //        {
    //            var realTimePrice = await _twelveDataClient.GetRealTimePriceAsync(info.Symbol);
    //            sb.AppendLine($"{++i}) {info.Symbol} : {realTimePrice.Price:0.00$}");
    //        }
    //        return sb.ToString();
    //    }
    //}
}
