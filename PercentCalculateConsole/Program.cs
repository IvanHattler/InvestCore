using Autofac;
using InvestCore.Domain.Models;
using InvestCore.PercentCalculateConsole.Domain;
using Microsoft.Extensions.Configuration;
using PercentCalculateConsole.IoC;
using PercentCalculateConsole.Services.Interfaces;

internal class Program
{
    private static async Task Main(string[] _)
    {
        var configuration = AppRegistry.BuildConfig();
        var telegramToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
            ?? string.Empty;
        var stockPortfolio = configuration.GetRequiredSection("StockPortfolioCalculationModel").Get<StockPortfolioCalculationModel>()
            ?? new StockPortfolioCalculationModel();
        var replenisment = configuration.GetRequiredSection("Replenishment").Get<ReplenishmentModel>()
            ?? new ReplenishmentModel();
        stockPortfolio.Replenishment = replenisment;
        stockPortfolio.TickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfo[]>()
            ?? Array.Empty<TickerInfo>();

        var container = AppRegistry.BuildContainer(telegramToken);

        var messageService = container.Resolve<IMessageService>();
        Console.WriteLine(await messageService.GetResultMessage(stockPortfolio));
        Console.ReadLine();
    }
}