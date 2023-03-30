using System.Globalization;
using Autofac;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PercentCalculateConsole.IoC;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;
using Timer = System.Threading.Timer;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
var configuration = AppRegistry.BuildConfig();
var needStartOneTime = configuration.GetRequiredSection("NeedStartOneTime").Get<bool>();

if (needStartOneTime)
{
    var telegramToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
            ?? string.Empty;
    var spreadsheetConfig = configuration.GetRequiredSection("SpreadsheetConfig").Get<SpreadsheetConfig>()
        ?? new SpreadsheetConfig();
    var tickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfo[]>()
        ?? Array.Empty<TickerInfo>();
    var replenisment = configuration.GetRequiredSection("Replenishment").Get<ReplenishmentModel>()
        ?? new ReplenishmentModel();
    var portfolioInvestment = configuration.GetRequiredSection("PortfolioInvestment").Get<PortfolioInvestmentModel>()
        ?? new PortfolioInvestmentModel();

    var container = AppRegistry.BuildContainer(telegramToken, spreadsheetConfig);
    var logger = container.Resolve<ILogger>();
    logger.LogInformation("Начало обработки: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());

    try
    {
        var workflowService = container.Resolve<IWorkflowService>();

        workflowService
            .UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment)
            .Wait();

        logger.LogInformation("Окончание обработки: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());
    }
    catch (Exception e)
    {
        logger.LogError(e, "Возникло исключение");
    }
}
else
{

    var intevalMs = configuration.GetRequiredSection("IntervalMs").Get<int>();
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
    var callback = new TimerCallback(async state =>
    {
        var telegramToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
            ?? string.Empty;
        var spreadsheetConfig = configuration.GetRequiredSection("SpreadsheetConfig").Get<SpreadsheetConfig>()
            ?? new SpreadsheetConfig();
        var tickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfo[]>()
            ?? Array.Empty<TickerInfo>();
        var replenisment = configuration.GetRequiredSection("Replenishment").Get<ReplenishmentModel>()
            ?? new ReplenishmentModel();
        var portfolioInvestment = configuration.GetRequiredSection("PortfolioInvestment").Get<PortfolioInvestmentModel>()
            ?? new PortfolioInvestmentModel();

        var container = AppRegistry.BuildContainer(telegramToken, spreadsheetConfig);


        var workflowService = container.Resolve<IWorkflowService>();

        var logger = container.Resolve<ILogger>();
        await workflowService
            .UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment);
        logger.LogInformation("---------------------Окончание обработки---------------------");
    });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

    Console.WriteLine($"Таймер запущен с интервалом {intevalMs / 1000} сек.");
    var timer = new Timer(callback, null, 0, intevalMs);
    Console.ReadLine();
}