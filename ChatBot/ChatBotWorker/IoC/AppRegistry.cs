using System.Reflection;
using Autofac;
using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using ChatBot.Data.Helpers;
using ChatBot.Data.Repositories;
using ChatBot.Shares.Integration.Services;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Tinkoff.InvestApi;
using IShareService = ChatBot.Core.Services.Interfaces.IShareService;

namespace ChatBotWorker.IoC
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

        public static void BuildContainer(ContainerBuilder builder)
        {
            var logFilePath = Path.Combine(FileHelper.DirectoryPath, "log.txt");
            var configuration = BuildConfig();

            var telegramToken = configuration.GetRequiredSection("TelegramToken").Get<string>();
            var availableIds = configuration.GetRequiredSection("AvailableIds").Get<long[]>();
            var messageInterval = configuration.GetRequiredSection("MessageIntervalMs").Get<double>();
            var needSendMessagesOnStart = configuration.GetRequiredSection("NeedSendMessagesOnStart").Get<bool>();
            var tinkoffToken = configuration.GetRequiredSection("TinkoffToken").Get<string>();

            #region Infrastructure

            builder.Register(c =>
                LoggerFactory.Create(x =>
                {
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
        }
    }
}
