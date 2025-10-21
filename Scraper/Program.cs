using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Scraper.Services;
using Scraper.Services.Implementations;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<ScraperService>();

        // 🔹 Worker que ouve a fila de requisições
        services.AddHostedService<ScraperRequestWorker>();
    })
    .Build()
    .Run();