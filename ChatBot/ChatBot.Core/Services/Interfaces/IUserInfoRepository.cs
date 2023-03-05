using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Interfaces
{
    public interface IUserInfoRepository
    {
        Task<bool> AddAsync(UserInfo userInfo);

        Task<UserInfo?> GetAsync(long userId);

        Task<bool> RemoveAsync(long id);

        Task<IEnumerable<UserInfo>> GetAllAsync();

        Task<bool> UpdateAsync(UserInfo userInfo);
    }
}
