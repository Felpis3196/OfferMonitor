using Scraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper.Services.Implementations
{
    public interface ISiteScraper
    {
        Task<List<OfferMessage>> ScrapeAsync(string url);
    }
}
