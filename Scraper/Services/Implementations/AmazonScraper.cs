using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Scraper.Models;

namespace Scraper.Services.Implementations
{
    public class AmazonScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.BinaryLocation = Environment.GetEnvironmentVariable("CHROME_BIN");

            using var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            while (true)
            {
                wait.Until(d => d.FindElements(By.CssSelector("div[data-component-type='s-search-result']")).Count > 0);

                var products = driver.FindElements(By.CssSelector("div[data-component-type='s-search-result']"));

                foreach (var product in products)
                {
                    try
                    {
                        var titleElem = product.FindElement(By.CssSelector("h2 a span"));
                        var linkElem = product.FindElement(By.CssSelector("h2 a"));
                        var priceElem = product.FindElements(By.CssSelector("span.a-price > span.a-offscreen")).FirstOrDefault();

                        if (priceElem == null) continue;

                        var title = titleElem.Text.Trim();
                        var priceText = priceElem.Text.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim();
                        decimal.TryParse(priceText, out var price);
                        var link = linkElem.GetAttribute("href");

                        Console.WriteLine($"[DEBUG] Produto: {title} - R${price}"); // log para debug

                        offers.Add(new OfferMessage
                        {
                            Title = title,
                            Price = price,
                            Url = link,
                            Store = "Amazon",
                            Category = "Geral"
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro no produto: {ex.Message}");
                        continue;
                    }
                }

                try
                {
                    var nextBtn = driver.FindElement(By.CssSelector("li.a-last a"));
                    nextBtn.Click();
                    await Task.Delay(5000); // espera JS carregar
                }
                catch
                {
                    break;
                }
            }

            driver.Quit();
            return offers;
        }
    }
}
