﻿using System.Reflection;
using Autofac;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PercentCalculateConsole.Services.Implementation;
using PercentCalculateConsole.Services.Interfaces;
using Tinkoff.InvestApi;

namespace PercentCalculateConsole.IoC
{
    public static class AppRegistry
    {
        public static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("configs/config.json", false)
                .AddJsonFile("configs/ticker-infos.json", false)
                .AddJsonFile("configs/tinkoff-token.json", false)
                .AddJsonFile("configs/replenishment.json", false)
                .Build();
        }

        public static IContainer BuildContainer(string tinkoffToken)
        {
            var builder = new ContainerBuilder();

            #region Infrastructure

            builder.Register(c =>
                LoggerFactory.Create(x =>
                {
                    x.AddConsole();
                    x.AddDebug();
                }).CreateLogger<ILogger>())
                .SingleInstance().As<ILogger>();
            builder.Register(c => new MemoryCache(new MemoryCacheOptions())).SingleInstance().As<IMemoryCache>();

            #endregion Infrastructure

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
