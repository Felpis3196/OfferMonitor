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

        public async Task RunScraperAsync(string url)
        {
            try
            {
                var scraper = GetScraperFromUrl(url);

                if (scraper == null)
                {
                    Console.WriteLine($"⚠️ Nenhum scraper compatível para: {url}");
                    return;
                }

                var offers = await scraper.ScrapeAsync(url);

                if (offers == null || !offers.Any())
                {
                    Console.WriteLine($"⚠️ Nenhuma oferta encontrada em {url}");
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

                Console.WriteLine($"✅ {offerInputs.Count} ofertas coletadas em {url}");

                _publisher.Publish(offerInputs);
                Console.WriteLine($"📦 Ofertas enviadas para o RabbitMQ com sucesso!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar {url}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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
