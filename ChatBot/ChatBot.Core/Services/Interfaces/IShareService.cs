using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Interfaces
{
    public interface IShareService : InvestCore.Domain.Services.Interfaces.IShareService
    {
        Task<IEnumerable<TickerInfoModel>> UpdateTickerInfosAsync(IEnumerable<TickerInfoModel> tickerInfos);
    }
}
