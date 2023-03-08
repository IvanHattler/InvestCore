using Autofac;
using InvestCore.PercentCalculateConsole.Domain;
using Microsoft.Extensions.Configuration;
using PercentCalculateConsole.IoC;
using PercentCalculateConsole.Services.Interfaces;

var configuration = AppRegistry.BuildConfig();
var telegramToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
    ?? string.Empty;
var stockPortfolio = configuration.GetRequiredSection("StockPortfolioCalculationModel").Get<StockPortfolioCalculationModel>()
    ?? new StockPortfolioCalculationModel();
stockPortfolio.TickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerInfo[]>()
    ?? new TickerInfo[0];
var container = AppRegistry.BuildContainer(telegramToken);

var messageService = container.Resolve<IMessageService>();
Console.WriteLine(messageService.GetResultMessage(stockPortfolio));
Console.ReadLine();
