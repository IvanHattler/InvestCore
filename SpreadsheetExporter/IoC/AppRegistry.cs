using System.Reflection;
using Autofac;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.SpreadsheetsApi.Services.Implementation;
using InvestCore.SpreadsheetsApi.Services.Interfaces;
using InvestCore.TinkoffApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadsheetExporter.Domain;
using SpreadsheetExporter.Services.Implementation;
using SpreadsheetExporter.Services.Interfaces;
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
                .AddJsonFile("configs/portfolio-investment.json", false)
                .Build();
        }

        public static IContainer BuildContainer(string tinkoffToken, LogLevel minimumLogLevel, SpreadsheetConfig spreadsheetConfig)
        {
            var builder = new ContainerBuilder();

            #region Infrastructure

            builder.Register(c =>
                LoggerFactory.Create(x =>
                {
                    x.AddConsole();
                    x.AddDebug();
                    x.SetMinimumLevel(minimumLogLevel);
                }).CreateLogger<ILogger>())
                .SingleInstance().As<ILogger>();
            builder.Register(c => new MemoryCache(new MemoryCacheOptions())).SingleInstance().As<IMemoryCache>();

            #endregion Infrastructure

            #region Services

            builder.RegisterType<TinkoffApiService>().SingleInstance().As<IShareService>();
            builder.RegisterType<WorkflowService>().SingleInstance().As<IWorkflowService>();
            builder.Register(c => InvestApiClientFactory.Create(tinkoffToken)).SingleInstance();
            builder.Register(c =>
            {
                using var stream = new FileStream("configs/client-secrets.json", FileMode.Open, FileAccess.Read);
                return new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleCredential
                        .FromStream(stream)
                        .CreateScoped(SpreadsheetConfig.Scopes),
                    ApplicationName = spreadsheetConfig.ApplicationName,
                });
            }).SingleInstance();
            builder.RegisterType<SpreadsheetService>().SingleInstance().As<ISpreadsheetService>();

            #endregion Services

            return builder.Build();
        }
    }
}
