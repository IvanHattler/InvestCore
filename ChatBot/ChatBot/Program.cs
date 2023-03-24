using Autofac;
using ChatBot.Core.Services.Interfaces;
using ChatBot.IoC;
using Microsoft.Extensions.Configuration;


var configuration = AppRegistry.BuildConfig();
var telegramToken = configuration.GetRequiredSection("TelegramToken").Get<string>();
var twelveDataApiToken = configuration.GetRequiredSection("TwelveDataApiToken").Get<string>();
var availableIds = configuration.GetRequiredSection("AvailableIds").Get<long[]>();
var messageInterval = configuration.GetRequiredSection("MessageIntervalMs").Get<double>();
var needSendMessagesOnStart = configuration.GetRequiredSection("NeedSendMessagesOnStart").Get<bool>();
var tinkoffToken = configuration.GetRequiredSection("TinkoffToken").Get<string>();
var container = AppRegistry.BuildContainer(twelveDataApiToken, telegramToken, availableIds, messageInterval, needSendMessagesOnStart, tinkoffToken);

var initService = container.Resolve<IInitService>();
await initService.Init();

var bot = container.Resolve<ITelegramBotService>();

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

await bot.SendStartedMessageAsync(cancellationToken);
await bot.StartReceivingAsync(cancellationToken);

Console.ReadLine();
