using System.Reflection;
using Autofac;
using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using ChatBot.Data.Helpers;
using ChatBot.Data.Repositories;
using ChatBot.Shares.Integration.Services;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tinkoff.InvestApi;
using IShareService = ChatBot.Core.Services.Interfaces.IShareService;

namespace ChatBot.IoC
{
    public static class AppRegistry
    {
        public static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("configs/config.json", false)
                .AddJsonFile("configs/tinkoff-token.json", false)
                .Build();
        }

        public static IContainer BuildContainer(string twelveDataApiToken, string telegramToken, long[] availableIds, double messageInterval, bool needSendMessagesOnStart,
            string tinkoffToken)
        {
            var logFilePath = Path.Combine(FileHelper.DirectoryPath, "log.txt");
            var builder = new ContainerBuilder();

            #region Infrastructure

            builder.Register(c =>
                LoggerFactory.Create(x =>
                {
                    x.AddConsole();
                    x.AddDebug();
                    x.AddFile(logFilePath, true);
                }).CreateLogger<ILogger>())
                .SingleInstance().As<ILogger>();
            builder.Register(c => new MemoryCache(new MemoryCacheOptions())).SingleInstance().As<IMemoryCache>();

            #endregion Infrastructure

            #region Repository

            builder.RegisterType<JsonUserInfoRepository>().SingleInstance().As<IUserInfoRepository>();

            #endregion Repository

            #region Services

            builder.RegisterType<UserService>().SingleInstance().As<IUserService>()
                .WithParameter("availableIds", availableIds);
            builder.RegisterType<TinkoffApiService>().SingleInstance().As<InvestCore.Domain.Services.Interfaces.IShareService>();
            builder.RegisterType<TinkoffApiShareService>().SingleInstance().As<IShareService>();
            builder.RegisterType<UserInfoService>().SingleInstance().As<IUserInfoService>();
            builder.RegisterType<SymbolsService>().SingleInstance().As<ISymbolsService>();
            builder.RegisterType<TelegramBotService>().SingleInstance().As<ITelegramBotService>()
                .WithParameter("token", telegramToken)
                .WithParameter("messageInterval", messageInterval)
                .WithProperty(nameof(TelegramBotService.NeedSendMessagesOnStart), needSendMessagesOnStart);
            builder.RegisterType<MessageService>().SingleInstance().As<IMessageService>();
            builder.RegisterType<InitService>().SingleInstance().As<IInitService>();
            builder.Register(c => InvestApiClientFactory.Create(tinkoffToken)).SingleInstance();

            #endregion Services

            return builder.Build();
        }
    }
}
