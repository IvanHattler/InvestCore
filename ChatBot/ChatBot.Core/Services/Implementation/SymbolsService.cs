using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Implementation
{
    public class SymbolsService : ISymbolsService
    {
        private readonly IUserInfoRepository _userInfoRepository;

        public SymbolsService(IUserInfoRepository userInfoRepository)
        {
            _userInfoRepository = userInfoRepository;
        }

        public async Task<IEnumerable<TickerInfoModel>> GetSymbolsAsync(long userId)
        {
            var infos = await _userInfoRepository.GetAsync(userId);
            return infos?.TickerInfos
                ?? Array.Empty<TickerInfoModel>();
        }
    }
}
