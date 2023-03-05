using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;

namespace InvestCore.Domain.Services.Implementation
{
    public class FakeShareService : IShareService
    {
        public Task<Dictionary<string, decimal>> GetCurrentOrLastPricesAsync(IEnumerable<(string, InstrumentType)> symbols)
        {
            return GetPricesAsync(symbols);
        }

        public Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<(string, InstrumentType)> symbols)
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
