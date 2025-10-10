
using Scraper.Services.Implementations;

namespace Scraper.Services
{
    public class ScraperFactory
    {
        public static ISiteScraper GetScraper(string url)
        {
            if (url.Contains("kabum", StringComparison.OrdinalIgnoreCase))
                return new KabumScraper();

            if (url.Contains("amazon", StringComparison.OrdinalIgnoreCase))
                return new AmazonScraper();

            if (url.Contains("magazineluiza", StringComparison.OrdinalIgnoreCase) || url.Contains("magalu", StringComparison.OrdinalIgnoreCase))
                return new MagaluScraper();

            return new GenericScraper();
        }
    }
}
