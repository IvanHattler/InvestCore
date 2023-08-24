using System.Diagnostics;
using System.Globalization;
using Autofac;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PercentCalculateConsole.IoC;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;

internal class Program
{
    private static async Task Main(string[] _)
    {
        var stopwatch = Stopwatch.StartNew();

        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        var configuration = AppRegistry.BuildConfig();

        var tinkoffToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
            ?? string.Empty;
        var spreadsheetConfig = configuration.GetRequiredSection("SpreadsheetConfig").Get<SpreadsheetConfig>()
            ?? new SpreadsheetConfig();
        var tickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfo[]>()
            ?? Array.Empty<TickerInfo>();
        var replenisment = configuration.GetRequiredSection("Replenishment").Get<ReplenishmentModel>()
            ?? new ReplenishmentModel();
        var portfolioInvestment = configuration.GetRequiredSection("PortfolioInvestment").Get<PortfolioInvestmentModel>()
            ?? new PortfolioInvestmentModel();

        var container = AppRegistry.BuildContainer(tinkoffToken, EnvironmentHelper.IsDebug ? LogLevel.Trace : LogLevel.Warning, spreadsheetConfig);
        var logger = container.Resolve<ILogger>();

        try
        {
            logger.LogInformation("Start processing: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());

            var workflowService = container.Resolve<IWorkflowService>();

            await workflowService
                .UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment);

            logger.LogInformation("End processing: {datetime}", DateTimeHelper.GetUTCPlus4DateTime());
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occured");
        }
        finally
        {
            stopwatch.Stop();
            logger.LogWarning("Processing ended by {elapsed}", stopwatch.Elapsed);
        }
    }
}
