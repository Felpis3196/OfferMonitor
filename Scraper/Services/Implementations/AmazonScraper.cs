using HtmlAgilityPack;
using Scraper.Models;

namespace Scraper.Services.Implementations
{
    public class AmazonScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@data-component-type, 's-search-result')]");

            if (nodes == null) return offers;

            foreach (var node in nodes)
            {
                var title = node.SelectSingleNode(".//span[@class='a-text-normal']")?.InnerText?.Trim() ?? "Sem título";
                var priceText = node.SelectSingleNode(".//span[@class='a-price-whole']")?.InnerText?.Trim() ?? "0";
                decimal.TryParse(priceText.Replace(".", "").Replace(",", "."), out var price);
                var link = node.SelectSingleNode(".//a[@class='a-link-normal']")?.GetAttributeValue("href", url) ?? url;

                offers.Add(new OfferMessage
                {
                    Title = title,
                    Price = price,
                    Url = link.StartsWith("http") ? $"https://www.amazon.com.br{link}" : link,
                    Store = "Amazon",
                    Category = "Geral"
                });
            }

            return offers;
        }
    }
}
