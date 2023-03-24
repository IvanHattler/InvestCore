using Autofac;
using Autofac.Extensions.DependencyInjection;
using ChatBotWorker.IoC;

IHost host = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .UseWindowsService(options =>
    {
        options.ServiceName = "winvidmgmt64";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<ChatBotWorker.ChatBotWorker>();
    })
    .ConfigureContainer<ContainerBuilder>(AppRegistry.BuildContainer)
    .Build();

await host.RunAsync();


