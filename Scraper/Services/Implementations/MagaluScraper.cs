using HtmlAgilityPack;
using Scraper.Models;

namespace Scraper.Services.Implementations
{
    public class MagaluScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'sc-APcvf')]");

            if (nodes == null) return offers;

            foreach (var node in nodes)
            {
                var title = node.SelectSingleNode(".//h2")?.InnerText?.Trim() ?? "Sem título";
                var priceText = node.SelectSingleNode(".//p[contains(@class, 'price__SalesPrice')]")?.InnerText?.Trim() ?? "0";
                decimal.TryParse(priceText.Replace("R$", "").Replace(",", ".").Trim(), out var price);
                var link = node.SelectSingleNode(".//a")?.GetAttributeValue("href", url) ?? url;

                offers.Add(new OfferMessage
                {
                    Title = title,
                    Price = price,
                    Url = link.StartsWith("http") ? link : $"https://www.magazineluiza.com.br{link}",
                    Store = "Magalu",
                    Category = "Eletrônicos"
                });
            }

            return offers;
        }
    }
}
