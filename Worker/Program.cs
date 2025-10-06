using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker.Worker>();
        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<ScraperService>();
    })
    .Build();

await host.RunAsync();
