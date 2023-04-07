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

var container = AppRegistry.BuildContainer(telegramToken, LogLevel.Warning, spreadsheetConfig);
var logger = container.Resolve<ILogger>();

try
{
    logger.LogInformation("Start processing: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());

    var workflowService = container.Resolve<IWorkflowService>();

    workflowService
        .UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment)
        .Wait();

    logger.LogInformation("End processing: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());
}
catch (Exception e)
{
    logger.LogError(e, "An exception occured");
}