using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Interfaces
{
    public interface IUserInfoService
    {
        Task<bool> HasSubscribersAsync();

        Task<bool> RemoveAsync(long userId);

        Task<bool> AddAsync(UserInfo userInfo);

        Task<IEnumerable<UserInfo>> GetAllAsync();

        Task<UserInfo?> GetAsync(long userId);

        Task SetDataMessageId(long userId, int? messageId);

        Task<bool> UpdateAsync(UserInfo userInfo);
    }
}
