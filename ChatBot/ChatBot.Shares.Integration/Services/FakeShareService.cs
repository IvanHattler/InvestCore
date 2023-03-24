using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;

namespace ChatBot.Shares.Integration.Services
{
    public class FakeShareService : BaseShareService, IShareService
    {
        public override Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> symbols)
        {
            return GetPricesAsync(symbols);
        }

        public override Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
        {
            var rand = new Random();
            var res = new Dictionary<string, decimal>(tickerInfos.Count());

            foreach (var tickerInfo in tickerInfos)
            {
                if (!res.ContainsKey(tickerInfo.Ticker))
                {
                    var price = (decimal)rand.Next(1, 4);
                    res.Add(tickerInfo.Ticker, price);
                }
            }
            return Task.FromResult(res);
        }
    }
}
