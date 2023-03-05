using System.Reflection;
using Autofac;
using InvestCore.Domain.Services.Implementation;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.PercentCalculateConsole.Services.Implementation;
using InvestCore.PercentCalculateConsole.Services.Interfaces;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tinkoff.InvestApi;

namespace InvestCore.PercentCalculateConsole.IoC
{
    public static class AppRegistry
    {
        public static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("config.json", false)
                .Build();
        }

        public static IContainer BuildContainer(string tinkoffToken)
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

            builder.RegisterType<MulMetricStrategy>().SingleInstance().As<IMetricStrategy>();
            builder.RegisterType<BuyModelService>().SingleInstance().As<IBuyModelService>();
            builder.RegisterType<TinkoffApiService>().SingleInstance().As<IShareService>();
            builder.RegisterType<StockPortfolioService>().SingleInstance().As<IStockPortfolioService>();
            builder.Register(c => InvestApiClientFactory.Create(tinkoffToken)).SingleInstance();
            builder.RegisterType<MessageService>().SingleInstance().As<IMessageService>();

            #endregion Services

            return builder.Build();
        }
    }
}
