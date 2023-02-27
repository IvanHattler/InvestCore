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

    static Program()
    {
        _configuration = BuildConfig();

        var telegramToken = _configuration.GetRequiredSection("TinkoffToken").Get<string>();
        _stockPortfolio = _configuration.GetRequiredSection("StockPortfolioCalculationModel").Get<StockPortfolioCalculationModel>()
            ?? new StockPortfolioCalculationModel();

        _container = BuildContainer(telegramToken);
    }

    private static void Main(string[] args)
    {
        var stockPortfolioService = _container.Resolve<IStockPortfolioService>();

        stockPortfolioService.LoadPricesToModel(_stockPortfolio);

        Console.WriteLine("--------------------------Месяц №0--------------------------");
        PrintOverall(_stockPortfolio);

        var buyModelService = _container.Resolve<IBuyModelService>();

        for (int i = 0; i < 12; i++)
        {
            var (message, model) = GetBuyMessage(buyModelService, _stockPortfolio);

            Console.WriteLine($"--------------------------Месяц №{i + 1}--------------------------");
            Console.WriteLine();
            Console.WriteLine(message);

            stockPortfolioService.UpdateOverallSum(_stockPortfolio, model);

            PrintOverall(_stockPortfolio);
        }
    }

    private static void PrintOverall(StockPortfolioCalculationModel stockPortfolio)
    {
        var overall = stockPortfolio.Share.OverallSum
            + stockPortfolio.GosBond.OverallSum
            + stockPortfolio.CorpBond.OverallSum;

        PrintOverall(stockPortfolio.Share.OverallSum,
            stockPortfolio.GosBond.OverallSum,
            stockPortfolio.CorpBond.OverallSum, overall);
    }

    private static IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddJsonFile("config.json", false)
            .Build();
    }

    private static void PrintOverall(decimal newOverallShares, decimal newOverallGosBonds, decimal newOverallCorpBonds, decimal newOverall)
    {
        Console.WriteLine();
        Console.WriteLine($"{newOverallShares:F}\t{newOverallGosBonds:F}\t{newOverallCorpBonds:F}");
        Console.WriteLine($"{newOverallShares / newOverall:P4}\t{newOverallGosBonds / newOverall:P4}\t{newOverallCorpBonds / newOverall:P4}");
        Console.WriteLine();
    }

    private static (string, BuyModel) GetBuyMessage(IBuyModelService buyModelService, StockPortfolioCalculationModel model)
    {
        var bestModel = buyModelService.CalculateBestBuyModelOptimized(model.Share, model.GosBond, model.CorpBond, model.Replenishment);

        if (bestModel == null)
            return ("Не удалось вычислить", bestModel);

        return (bestModel.GetBuyMessage(), bestModel);
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

        #endregion Services

        return builder.Build();
    }
}
