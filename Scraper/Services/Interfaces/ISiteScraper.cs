using Scraper.Models;

namespace Scraper.Services.Implementations
{
    public interface ISiteScraper
    {
        Task<List<OfferMessage>> ScrapeAsync(string url);
    }
}
