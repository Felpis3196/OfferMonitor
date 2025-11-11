using Scraper.Models;
using Scraper.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace Scraper.Services.Implementations
{
    public class AmazonScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var seleniumUrl = Environment.GetEnvironmentVariable("SELENIUM_URL")
                              ?? "http://selenium:4444/wd/hub";

            LoggingHelper.Log($"🚀 Conectando ao Selenium remoto: {seleniumUrl}", "INFO");

            try
            {
                var options = new ChromeOptions();
                options.AddArguments(
                    "--headless=new",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-extensions",
                    "--disable-blink-features=AutomationControlled",
                    "--blink-settings=imagesEnabled=false",
                    "--window-size=1920,1080",
                    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0 Safari/537.36"
                );
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                using var driver = new RemoteWebDriver(
                    new Uri(seleniumUrl),
                    options.ToCapabilities(),
                    TimeSpan.FromSeconds(60));

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

                LoggingHelper.Log($"🌐 Acessando: {url}", "INFO");
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
                wait.Until(d => d.FindElements(By.XPath("//*[contains(text(),'R$')]")).Any());

                var js = (IJavaScriptExecutor)driver;

                // --- Scroll progressivo para carregar todos os produtos ---
                long lastHeight = (long)(js.ExecuteScript("return document.body.scrollHeight"));
                for (int i = 0; i < 20; i++)
                {
                    js.ExecuteScript("window.scrollBy(0, 1000);");
                    await Task.Delay(800);
                    long newHeight = (long)(js.ExecuteScript("return document.body.scrollHeight"));
                    if (newHeight == lastHeight)
                        break;
                    lastHeight = newHeight;
                }

                // Aguarda renderização final do JS
                await Task.Delay(2000);

#pragma warning disable CS8600
                var script = @"
                    const selectors = [
                        'div[data-component-type=""s-search-result""]',
                        'div[data-testid=""product-card""]',
                        'div[data-asin][data-index]',
                        'div.puis-card-container',
                        'div.a-section.a-spacing-base',
                        'li.a-carousel-card'
                    ];

                    const containers = Array.from(document.querySelectorAll(selectors.join(',')))
                        .filter(e => e.innerText.includes('R$'));

                    return containers.map(e => {
                        const titleElem = e.querySelector('h2 a span, span.a-size-medium, span.a-size-base-plus, .a-truncate-full');
                        const title = titleElem ? titleElem.innerText.trim() : 'Sem título';

                        const priceElem = e.querySelector('span.a-price > span.a-offscreen, span.a-color-price');
                        let priceText = priceElem ? priceElem.innerText : '';
                        priceText = priceText.replace(/[^\d,]/g, '').replace(',', '.');

                        const linkElem = e.querySelector('a.a-link-normal, a.a-color-base, a.a-text-normal, a[data-testid=""product-card-link""]');
                        const link = linkElem ? (linkElem.href || linkElem.getAttribute('href')) : '';

                        const brandElem = e.querySelector('span[id^=""byline-""], .a-row.a-size-base.a-color-secondary');
                        const brand = brandElem ? brandElem.innerText.trim() : '';

                        const ratingElem = e.querySelector('i.a-icon-star-small span, span[aria-label*=""de 5 estrelas""], span.a-icon-alt');
                        const rating = ratingElem ? ratingElem.innerText.replace(' de 5 estrelas','').trim() : '';

                        return { title, price: priceText, link, brand, rating };
                    });
                ";
                var data = (IReadOnlyCollection<object>)js.ExecuteScript(script);
#pragma warning restore CS8600

                LoggingHelper.Log($"📦 {data.Count} produtos capturados via JS.", "INFO");

                var seen = new HashSet<string>();

                foreach (var obj in data)
                {
                    var dict = (Dictionary<string, object?>)obj;
                    string title = dict["title"]?.ToString()?.Trim() ?? "Sem título";
                    string link = dict["link"]?.ToString() ?? "";

                    if (string.IsNullOrEmpty(link)) continue;
                    if (title.Equals("Sem título", StringComparison.OrdinalIgnoreCase)) continue;

                    decimal.TryParse(dict["price"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
                    if (price <= 0) continue;

                    string key = $"{title}|{price}";
                    if (!seen.Add(key)) continue; // evita duplicado

                    var brand = dict["brand"]?.ToString() ?? "";
                    var rating = dict["rating"]?.ToString() ?? "";

                    if (!link.StartsWith("http"))
                        link = $"https://www.amazon.com.br{link}";

                    offers.Add(new OfferMessage
                    {
                        Title = title,
                        Price = price,
                        Url = link,
                        Store = "Amazon",
                        Category = brand,
                        Discount = rating
                    });

                    LoggingHelper.Log($"✅ {title} - R${price} ({brand}) ★{rating}", "SUCCESS");
                }

                driver.Quit();
                LoggingHelper.Log("🧹 Selenium remoto finalizado.", "INFO");
            }
            catch (Exception ex)
            {
                LoggingHelper.Log($"❌ ERRO no Selenium remoto: {ex.Message}", "ERROR");
            }

            LoggingHelper.Log($"🔎 Total de produtos válidos: {offers.Count}", "INFO");
            return offers;
        }
    }
}
