using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;

namespace InvestCore.Domain.Services.Implementation
{
    public class FakeShareService : IShareService
    {
        public Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
        {
            return GetPricesAsync(tickerInfos);
        }

        public Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<TickerInfoBase> tickerInfos)
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

        public Task<decimal?> GetUSDRUBAsync()
        {
            return Task.FromResult((decimal?)75m);
        }
    }
}
