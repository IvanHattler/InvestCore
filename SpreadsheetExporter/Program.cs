using Autofac;
using InvestCore.Domain.Models;
using Microsoft.Extensions.Configuration;
using PercentCalculateConsole.IoC;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Interfaces;


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

await workflowService.UpdateTableAsync(tickerInfos, spreadsheetConfig, replenisment, portfolioInvestment);
