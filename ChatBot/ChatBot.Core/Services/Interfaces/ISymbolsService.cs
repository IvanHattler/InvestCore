using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Interfaces
{
    public interface ISymbolsService
    {
        Task<IEnumerable<TickerInfoModel>> GetSymbolsAsync(long userId);
    }
}
