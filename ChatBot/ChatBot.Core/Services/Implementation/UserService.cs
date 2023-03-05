using ChatBot.Core.Services.Interfaces;

namespace ChatBot.Core.Services.Implementation
{
    public class UserService : IUserService
    {
        private IEnumerable<long> _availableIds;

        public UserService(IEnumerable<long> availableIds)
        {
            _availableIds = availableIds;
        }

        public IEnumerable<long> GetAllowedUserIds()
        {
            return _availableIds;
        }
    }
}
