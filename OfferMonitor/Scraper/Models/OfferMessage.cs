namespace Scraper.Models;

public class OfferMessage
{
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string Url { get; set; } = "";
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;
    public string Store { get; internal set; }
    public string Category { get; internal set; }
    public decimal? OldPrice { get; internal set; }
    public string Discount { get; internal set; }
}
