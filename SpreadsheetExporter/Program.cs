using Autofac;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PercentCalculateConsole.IoC;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;
using Timer = System.Threading.Timer;

var configuration = AppRegistry.BuildConfig();
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
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
var callback = new TimerCallback(async state =>
{
    await workflowService
        .UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment);
    logger.LogInformation("---------------------Окончание обработки---------------------");
});
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
var timer = new Timer(callback, null, 0, 60*60*1000);
Console.ReadLine();