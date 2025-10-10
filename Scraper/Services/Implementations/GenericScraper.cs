using HtmlAgilityPack;
using Scraper.Models;

namespace Scraper.Services.Implementations
{
    public class GenericScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var nodes = doc.DocumentNode.SelectNodes("//a[contains(@href, 'produto') or contains(@href, 'item')]");

            if (nodes == null) return offers;

            foreach (var node in nodes)
            {
                var title = node.InnerText.Trim();
                offers.Add(new OfferMessage
                {
                    Title = title,
                    Url = node.GetAttributeValue("href", url),
                    Store = "Desconhecida",
                    Category = "Geral",
                    Price = 0
                });
            }

            return offers;
        }
    }
}
