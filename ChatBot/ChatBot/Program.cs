using System.Reflection;
using Autofac;
using ChatBot.Core.Services.Implementation;
using ChatBot.Core.Services.Interfaces;
using ChatBot.Data.Helpers;
using ChatBot.Data.Repositories;
using ChatBot.Shares.Integration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tinkoff.InvestApi;

internal class Program
{
    private static readonly IContainer Container;
    private static readonly IConfigurationRoot Configuration;
    private static readonly string LogFilePath;

    static Program()
    {
        LogFilePath = Path.Combine(FileHelper.DirectoryPath, "log.txt");

        Configuration = BuildConfig();

        var telegramToken = Configuration.GetRequiredSection("TelegramToken").Get<string>();
        var twelveDataApiToken = Configuration.GetRequiredSection("TwelveDataApiToken").Get<string>();
        var availableIds = Configuration.GetRequiredSection("AvailableIds").Get<long[]>();
        var messageInterval = Configuration.GetRequiredSection("MessageIntervalMs").Get<double>();
        var needSendMessagesOnStart = Configuration.GetRequiredSection("NeedSendMessagesOnStart").Get<bool>();
        var tinkoffToken = Configuration.GetRequiredSection("TinkoffToken").Get<string>();

        Container = BuildContainer(twelveDataApiToken, telegramToken, availableIds, messageInterval, needSendMessagesOnStart, tinkoffToken);
    }

    private static async Task Main(string[] args)
    {
        var initService = Container.Resolve<IInitService>();
        await initService.Init();

        var bot = Container.Resolve<ITelegramBotService>();

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        await bot.SendStartedMessageAsync(cancellationToken);
        await bot.StartReceivingAsync(cancellationToken);

        Console.ReadLine();
    }

    private static IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddJsonFile("config.json", false)
            .Build();
    }

    private static IContainer BuildContainer(string twelveDataApiToken, string telegramToken, long[] availableIds, double messageInterval, bool needSendMessagesOnStart,
        string tinkoffToken)
    {
        var builder = new ContainerBuilder();

        #region Logger

        builder.Register(c =>
            LoggerFactory.Create(x =>
                {
                    x.AddConsole();
                    x.AddDebug();
                    x.AddFile(LogFilePath, true);
                }).CreateLogger<ILogger>())
            .SingleInstance().As<ILogger>();

        #endregion Logger

        #region Repository

        builder.RegisterType<JsonUserInfoRepository>().SingleInstance().As<IUserInfoRepository>();

        #endregion Repository

        #region Services

        builder.RegisterType<UserService>().SingleInstance().As<IUserService>()
            .WithParameter("availableIds", availableIds);
        builder.RegisterType<InvestCore.TinkoffApi.Services.TinkoffApiService>().SingleInstance().As<InvestCore.Domain.Services.Interfaces.IShareService>();
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
