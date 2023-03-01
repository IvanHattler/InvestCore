using InvestCore.Domain.Models;

namespace InvestCore.Domain.Services.Interfaces
{
    public interface IShareService
    {
        Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<(string, InstrumentType)> symbols);
        Task<Dictionary<string, decimal>> GetClosePricesAsync(IEnumerable<(string, InstrumentType)> symbols);
    }
}
