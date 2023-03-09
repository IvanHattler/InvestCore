using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Implementation
{
    public abstract class BaseShareService : IShareService
    {
        public abstract Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> symbols);

        public abstract Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<(string, InstrumentType)> symbols);

        public async Task<IEnumerable<TickerInfoModel>> UpdateTickerInfosAsync(IEnumerable<TickerInfoModel> tickerInfos)
        {
            Dictionary<string, decimal> prices = await GetPricesAsync(tickerInfos.Select(x => (x.Ticker, x.TickerType)));

            foreach (var tickerInfo in tickerInfos)
            {
                if (prices.TryGetValue(tickerInfo.Ticker, out var price))
                {
                    tickerInfo.LastPrice = tickerInfo.Price;
                    tickerInfo.Price = price;
                }
            }

            return tickerInfos;
        }
    }
}
