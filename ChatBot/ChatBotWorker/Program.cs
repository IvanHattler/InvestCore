using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using ChatBot.Data.Helpers;
using ChatBot.Data.Repositories;
using ChatBot.Shares.Integration.Services;
using Tinkoff.InvestApi;

namespace ChatBotWorker
{
    public class Program
    {
        private static string _logFilePath;
        private static string _telegramToken;
        private static string _twelveDataApiToken;
        private static long[] _availableIds;
        private static double _messageInterval;
        private static bool _needSendMessagesOnStart;
        private static string _tinkoffToken;

        public static Task Main(string[] args)
        {
            _logFilePath = Path.Combine(FileHelper.DirectoryPath, "log.txt");
            var configuration = BuildConfig();

            _telegramToken = configuration.GetRequiredSection("TelegramToken").Get<string>();
            _twelveDataApiToken = configuration.GetRequiredSection("TwelveDataApiToken").Get<string>();
            _availableIds = configuration.GetRequiredSection("AvailableIds").Get<long[]>();
            _messageInterval = configuration.GetRequiredSection("MessageIntervalMs").Get<double>();
            _needSendMessagesOnStart = configuration.GetRequiredSection("NeedSendMessagesOnStart").Get<bool>();
            _tinkoffToken = configuration.GetRequiredSection("TinkoffToken").Get<string>();

            IHost host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseWindowsService(options =>
                {
                    options.ServiceName = "winvidmgmt64";
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ChatBotWorker>();
                })
                .ConfigureContainer<ContainerBuilder>(BuildContainer)
                .Build();

            return host.RunAsync();
        }

        private static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("config.json", false)
                .Build();
        }

        private static void BuildContainer(ContainerBuilder builder)
        {
            #region Logger

            builder.Register(c =>
                LoggerFactory.Create(x =>
                {
                    x.AddFile(_logFilePath, true);
                }).CreateLogger<ILogger>())
                .SingleInstance().As<ILogger>();

            #endregion Logger

            #region Repository

            builder.RegisterType<JsonUserInfoRepository>().SingleInstance().As<IUserInfoRepository>();

            #endregion Repository

            #region Services

            builder.RegisterType<UserService>().SingleInstance().As<IUserService>()
                .WithParameter("availableIds", _availableIds);
            builder.RegisterType<InvestCore.TinkoffApi.Services.TinkoffApiService>().SingleInstance().As<InvestCore.Domain.Services.Interfaces.IShareService>();
            builder.RegisterType<TinkoffApiShareService>().SingleInstance().As<IShareService>();
            builder.RegisterType<UserInfoService>().SingleInstance().As<IUserInfoService>();
            builder.RegisterType<SymbolsService>().SingleInstance().As<ISymbolsService>();
            builder.RegisterType<TelegramBotService>().SingleInstance().As<ITelegramBotService>()
                .WithParameter("token", _telegramToken)
                .WithParameter("messageInterval", _messageInterval)
                .WithProperty(nameof(TelegramBotService.NeedSendMessagesOnStart), _needSendMessagesOnStart);
            builder.RegisterType<MessageService>().SingleInstance().As<IMessageService>();
            builder.RegisterType<InitService>().SingleInstance().As<IInitService>();
            builder.Register(c => InvestApiClientFactory.Create(_tinkoffToken)).SingleInstance();

            #endregion Services
        }
    }
}
