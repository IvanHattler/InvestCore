using System.Text.Json;
using ChatBot.Core.Services.Interfaces;
using ChatBot.Data.Helpers;
using InvestCore.Domain.Models;
using File = System.IO.File;

namespace ChatBot.Data.Repositories
{
    public class JsonUserInfoRepository : IUserInfoRepository
    {
        protected const string FileName = "userinfos.json";
        protected readonly string FilePath;

        public JsonUserInfoRepository()
        {
            if (!new DirectoryInfo(FileHelper.DirectoryPath).Exists)
            {
                Directory.CreateDirectory(FileHelper.DirectoryPath);
            }

            FilePath = Path.Combine(FileHelper.DirectoryPath, FileName);
        }

        public async Task<bool> AddAsync(UserInfo userInfo)
        {
            var infos = await GetAllAsync();

            if (infos.Select(x => x.UserId).Contains(userInfo.UserId))
                return false;

            var newInfos = infos.Append(userInfo);

            var json = JsonSerializer.Serialize(newInfos,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                });

            await File.WriteAllTextAsync(FilePath, json);

            return true;
        }

        public async Task<bool> RemoveAsync(long userId)
        {
            var infos = await GetAllAsync();

            if (!infos.Select(x => x.UserId).Contains(userId))
                return false;

            var newInfos = infos.Where(x => x.UserId != userId);

            var json = JsonSerializer.Serialize(newInfos,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                });

            await File.WriteAllTextAsync(FilePath, json);

            return true;
        }

        public async Task<IEnumerable<UserInfo>> GetAllAsync()
        {
            try
            {
                if (!new FileInfo(FilePath).Exists)
                    return Array.Empty<UserInfo>();

                var json = await File.ReadAllTextAsync(FilePath);

                return JsonSerializer.Deserialize<IEnumerable<UserInfo>>(json)
                    ?? Array.Empty<UserInfo>();
            }
            catch (Exception)
            {
                return Array.Empty<UserInfo>();
            }
        }

        public async Task<UserInfo?> GetAsync(long userId)
        {
            var infos = await GetAllAsync();

            return infos.FirstOrDefault(x => x.UserId == userId);
        }

        public async Task<bool> UpdateAsync(UserInfo userInfo)
        {
            var infos = await GetAllAsync();

            if (!infos.Where(x => x.UserId == userInfo.UserId).Any())
                return false;

            var newInfos = infos
                .Where(x => x.UserId != userInfo.UserId)
                .Append(userInfo);

            var json = JsonSerializer.Serialize(newInfos,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                });

            await File.WriteAllTextAsync(FilePath, json);

            return true;
        }
    }
}
