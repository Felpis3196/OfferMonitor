using HtmlAgilityPack;
using Application.Dto;
using Microsoft.Extensions.Configuration;

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
                Console.WriteLine("Nenhum site configurado para scraping.");
                return;
            }

            foreach (var site in sites)
            {
                Console.WriteLine($"Iniciando scraper em: {site}");
                await RunScraperAsync(site);
            }
        }

        public async Task RunScraperAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var title = doc.DocumentNode.SelectSingleNode("//div[@class='offer']/h2")?.InnerText?.Trim() ?? "Sem título";
            var priceText = doc.DocumentNode.SelectSingleNode("//div[@class='offer']/span[@class='price']")?.InnerText?.Trim() ?? "0";
            decimal.TryParse(priceText.Replace("R$", ""), out var price);

            var offers = new List<OfferInput>
            {
                new OfferInput
                {
                    Title = title,
                    Price = price,
                    Url = url,
                    Store = GetStoreNameFromUrl(url),
                    Category = "Eletrônicos"
                }
            };

            _publisher.Publish(offers);
            await Task.CompletedTask;
        }

        private string GetStoreNameFromUrl(string url)
        {
            if (url.Contains("amazon")) return "Amazon";
            if (url.Contains("kabum")) return "Kabum";
            if (url.Contains("pichau")) return "Pichau";
            return "Loja Desconhecida";
        }
    }
}
