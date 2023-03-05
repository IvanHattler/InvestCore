using System.Reflection;
using Autofac;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using InvestCore.Domain.Services.Implementation;
using InvestCore.Domain.Services.Interfaces;
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
                .AddJsonFile("config.json", false)
                .Build();
        }

        public static IContainer BuildContainer(string tinkoffToken, SpreadsheetConfig spreadsheetConfig)
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

            builder.RegisterType<FakeShareService>().SingleInstance().As<IShareService>();
            builder.RegisterType<WorkflowService>().SingleInstance().As<IWorkflowService>();
            builder.Register(c => InvestApiClientFactory.Create(tinkoffToken)).SingleInstance();
            builder.Register(c =>
            {
                using var stream = new FileStream("client-secrets.json", FileMode.Open, FileAccess.Read);
                return new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleCredential
                        .FromStream(stream)
                        .CreateScoped(spreadsheetConfig.Scopes),
                    ApplicationName = spreadsheetConfig.ApplicationName,
                });
            }).SingleInstance();

            #endregion Services

            return builder.Build();
        }
    }
}
