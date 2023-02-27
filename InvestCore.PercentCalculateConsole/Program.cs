using System.Reflection;
using Autofac;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Domain;
using InvestCore.PercentCalculateConsole.Services.Implementation;
using InvestCore.PercentCalculateConsole.Services.Interfaces;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tinkoff.InvestApi;

internal class Program
{
    private static readonly IContainer _container;
    private static readonly IConfigurationRoot _configuration;
    private static readonly StockPortfolioCalculationModel _stockPortfolio;
    private static readonly IMessageService _messageService;

    static Program()
    {
        _configuration = BuildConfig();

        var telegramToken = _configuration.GetRequiredSection("TinkoffToken").Get<string>()
            ?? string.Empty;
        _stockPortfolio = _configuration.GetRequiredSection("StockPortfolioCalculationModel").Get<StockPortfolioCalculationModel>()
            ?? new StockPortfolioCalculationModel();

        _container = BuildContainer(telegramToken);
        _messageService = _container.Resolve<IMessageService>();
    }

    private static void Main(string[] args)
    {
        Console.WriteLine(_messageService.GetResultMessage(_stockPortfolio));
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

    private static IContainer BuildContainer(string tinkoffToken)
    {
        var builder = new ContainerBuilder();

        #region Logger

        builder.Register(c =>
            LoggerFactory.Create(x =>
            {
                x.AddConsole();
                x.AddDebug();
            }).CreateLogger<ILogger>())
            .SingleInstance().As<ILogger>();

        #endregion Logger

        #region Services

        builder.RegisterType<SelectBestBuyModelStrategyByMul>().SingleInstance().As<ISelectBestBuyModelStrategy>();
        builder.RegisterType<BuyModelService>().SingleInstance().As<IBuyModelService>();
        builder.RegisterType<TinkoffApiService>().SingleInstance().As<IShareService>();
        builder.RegisterType<StockPortfolioService>().SingleInstance().As<IStockPortfolioService>();
        builder.Register(c => InvestApiClientFactory.Create(tinkoffToken)).SingleInstance();
        builder.RegisterType<MessageService>().SingleInstance().As<IMessageService>();

        #endregion Services

        return builder.Build();
    }
}
