using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;

namespace ChatBot.Shares.Integration.Services
{
    public class FakeShareService : BaseShareService, IShareService
    {
        public override Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<TickerInfoBase> symbols)
        {
            return GetPricesAsync(symbols.Select(x => (x.Ticker, x.TickerType)));
        }

        public override Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<(string, InstrumentType)> symbols)
        {
            var rand = new Random();
            var res = new Dictionary<string, decimal>(symbols.Count());

            foreach (var (symbol, _) in symbols)
            {
                if (!res.ContainsKey(symbol))
                {
                    var price = (decimal)rand.Next(1, 4);
                    res.Add(symbol, price);
                }
            }
            return Task.FromResult(res);
        }
    }
}
