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
var container = AppRegistry.BuildContainer(telegramToken, spreadsheetConfig);
var tickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfoWithCount[]>()
    ?? new TickerInfoWithCount[0];


var workflowService = container.Resolve<IWorkflowService>();

await workflowService.UpdateAsync(tickerInfos, spreadsheetConfig);
