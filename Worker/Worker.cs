using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scraper.Services;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ScraperService _scraperService;

        public Worker(ILogger<Worker> logger, ScraperService scraperService)
        {
            _logger = logger;
            _scraperService = scraperService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando o Worker e executando todos os scrapers...");

            try
            {
                await _scraperService.RunAllScrapersAsync();
                _logger.LogInformation("Scrapers executados com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar scrapers.");
            }

            // Loop simples para manter o worker vivo
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker rodando às: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
