using Application.Dto;
using Microsoft.Extensions.Configuration;
using Scraper.Services.Implementations;

namespace Scraper.Services
{
    public class ScraperService
    {
        private readonly RabbitMqPublisher _publisher;
        private readonly IConfiguration _config;

        public ScraperService(RabbitMqPublisher publisher, IConfiguration config)
        {
            _publisher = publisher;
            _config = config;
        }

        public async Task RunAllScrapersAsync()
        {
            var sites = _config.GetSection("ScraperConfig:Sites").Get<string[]>();

            if (sites == null || sites.Length == 0)
            {
                Console.WriteLine("❌ Nenhum site configurado para scraping.");
                return;
            }

            foreach (var site in sites)
            {
                Console.WriteLine($"\n===============================");
                Console.WriteLine($"🚀 Iniciando scraper em: {site}");
                Console.WriteLine($"===============================\n");

                await RunScraperAsync(site);
            }
        }

        public async Task RunScraperAsync(string url, ScrapingLogger? logger = null)
        {
            try
            {
                // Define o logger no contexto para uso pelos scrapers
                LoggingHelper.SetLogger(logger);
                
                logger?.Log($"🔍 Identificando scraper adequado para: {url}", "INFO");
                var scraper = GetScraperFromUrl(url);

                if (scraper == null)
                {
                    var message = $"⚠️ Nenhum scraper compatível para: {url}";
                    LoggingHelper.Log(message, "WARNING");
                    return;
                }

                var scraperName = scraper.GetType().Name.Replace("Scraper", "");
                LoggingHelper.Log($"✅ Scraper {scraperName} selecionado", "SUCCESS");
                LoggingHelper.Log("🚀 Iniciando processo de scraping...", "INFO");

                // Os scrapers usarão LoggingHelper.Log que automaticamente usa o logger
                var offers = await scraper.ScrapeAsync(url);
                
                // Limpa o logger do contexto
                LoggingHelper.SetLogger(null);

                if (offers == null || !offers.Any())
                {
                    var message = $"⚠️ Nenhuma oferta encontrada em {url}";
                    LoggingHelper.Log(message, "WARNING");
                    return;
                }

                var offerInputs = offers.Select(o => new OfferInput
                {
                    Title = o.Title,
                    Url = o.Url,
                    Store = o.Store,
                    Category = o.Category,
                    Price = o.Price
                }).ToList();

                var successMessage = $"✅ {offerInputs.Count} ofertas coletadas em {url}";
                LoggingHelper.Log(successMessage, "SUCCESS");

                LoggingHelper.Log("📤 Enviando ofertas para o RabbitMQ...", "INFO");
                _publisher.Publish(offerInputs);
                
                var publishMessage = $"📦 {offerInputs.Count} ofertas enviadas para o RabbitMQ com sucesso!";
                LoggingHelper.Log(publishMessage, "SUCCESS");
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ Erro ao processar {url}: {ex.Message}";
                LoggingHelper.Log(errorMessage, "ERROR");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    LoggingHelper.Log($"Stack trace: {ex.StackTrace}", "ERROR");
                }
            }
            finally
            {
                // Limpa o logger do contexto
                LoggingHelper.SetLogger(null);
            }
        }

        private ISiteScraper? GetScraperFromUrl(string url)
        {
            url = url.ToLower();

            if (url.Contains("amazon"))
                return new AmazonScraper();
            if (url.Contains("kabum"))
                return new KabumScraper();
            if (url.Contains("magalu"))
                return new MagaluScraper();

            return new GenericScraper();
        }
    }
}
