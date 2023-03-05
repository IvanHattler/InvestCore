using InvestCore.Domain.Models;

namespace ChatBot.Core.Services.Interfaces
{
    public interface IMessageService
    {
        public Task<string> GetMainTableMessageAsync(UserInfo userInfo);
    }
}
