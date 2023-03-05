namespace ChatBot.Core.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<long> GetAllowedUserIds();
    }
}
