using ChatBot.Core.Services.Interfaces;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ChatBot.Core.Services.Implementation
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly ILogger _logger;

        public UserInfoService(ILogger logger, IUserInfoRepository chatRepository)
        {
            _userInfoRepository = chatRepository;
            _logger = logger;
        }

        public async Task<bool> RemoveAsync(long userId)
        {
            var res = await _userInfoRepository.RemoveAsync(userId);
            _logger.LogInformation("Chat id ({userId}) deleted", userId);
            return res;
        }

        public async Task<bool> AddAsync(UserInfo userInfo)
        {
            var res = await _userInfoRepository.AddAsync(userInfo);
            _logger.LogInformation("UserInfo saved (id = {userId})", userInfo.UserId);
            return res;
        }

        public Task<IEnumerable<UserInfo>> GetAllAsync()
        {
            return _userInfoRepository.GetAllAsync();
        }

        public Task<UserInfo?> GetAsync(long userId)
        {
            return _userInfoRepository.GetAsync(userId);
        }

        public async Task<bool> HasSubscribersAsync()
        {
            var ids = (await _userInfoRepository.GetAllAsync())
                .Where(x => x.IsSubscribed);
            return ids != null && ids.Any();
        }

        public async Task SetDataMessageId(long userId, int? messageId)
        {
            var userInfo = await _userInfoRepository.GetAsync(userId);
            if (userInfo != null)
            {
                userInfo.DataMessageId = messageId;
                await _userInfoRepository.UpdateAsync(userInfo);
            }
        }

        public Task<bool> UpdateAsync(UserInfo userInfo)
        {
            return _userInfoRepository.UpdateAsync(userInfo);
        }
    }
}
