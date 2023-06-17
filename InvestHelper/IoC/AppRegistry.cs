using System.Reflection;
using InvestCore.DataLayer;
using InvestCore.Domain.Helpers;
using InvestCore.Domain.Models;
using InvestCore.Domain.Services.Interfaces;
using InvestCore.TinkoffApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Tinkoff.InvestApi;

namespace InvestHelper.IoC
{
    public static class AppRegistry
    {
        public static IConfigurationRoot BuildConfig(string[] args)
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("configs/ticker-infos.json", false)
                .AddJsonFile("configs/tinkoff-token.json", false)
                .AddJsonFile("configs/connection-strings.json", false)
                .Build();
        }

        public static WebApplication GetApplication(string[] args)
        {
            var configuration = BuildConfig(args);
            var tinkoffToken = configuration.GetRequiredSection("TinkoffToken").Get<string>()
                ?? string.Empty;
            var tickerInfos = configuration.GetRequiredSection("TickerInfos").Get<TickerPriceInfo[]>() ?? Array.Empty<TickerPriceInfo>();
            var connectionString = configuration.GetConnectionString("Default");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<ILogger>(c =>
                LoggerFactory.Create(x =>
                {
                    x.AddConsole();
                    x.AddDebug();
                    x.SetMinimumLevel(EnvironmentHelper.IsDebug ? LogLevel.Trace : LogLevel.Warning);
                }).CreateLogger<ILogger>())
                .AddSingleton<IShareService, TinkoffApiService>()
                .AddSingleton(c => InvestApiClientFactory.Create(tinkoffToken))
                .AddSwaggerGen()
                .AddMemoryCache()
                .AddSingleton<IEnumerable<TickerPriceInfo>>(x => tickerInfos)
                .AddDbContextFactory<BaseDbContext>(x => x.UseNpgsql(connectionString))
                .AddControllersWithViews();

            var app = builder.Build();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseStaticFiles();
            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

            app.MapFallbackToFile("index.html");
            return app;
        }
    }
}
