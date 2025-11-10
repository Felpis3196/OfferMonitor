using Scraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Scraper.Services.Implementations
{
    public class GenericScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var seleniumUrl = Environment.GetEnvironmentVariable("SELENIUM_URL") ?? "http://selenium:4444/wd/hub";

            Console.WriteLine($"🚀 Conectando ao Selenium remoto: {seleniumUrl}");

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

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(40);

                Console.WriteLine($"🌐 Acessando: {url}");
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                var js = (IJavaScriptExecutor)driver;

                // --- Scroll progressivo para carregar produtos dinâmicos ---
                long lastHeight = (long)(js.ExecuteScript("return document.body.scrollHeight"));
                for (int i = 0; i < 15; i++)
                {
                    js.ExecuteScript("window.scrollBy(0, 1200);");
                    await Task.Delay(700);
                    long newHeight = (long)(js.ExecuteScript("return document.body.scrollHeight"));
                    if (newHeight == lastHeight) break;
                    lastHeight = newHeight;
                }

                await Task.Delay(1500);

                #pragma warning disable CS8600
                var script = @"
                    const currencyRe = /R\$\s*\d/;
                    const nodes = Array.from(document.querySelectorAll('div.product-item, li.product-item, article.product-item, .product-card, .product, li, article, div, section'));
                    const productContainers = nodes.filter(e => currencyRe.test(e.innerText) && e.querySelector('a[href]'));

                    return productContainers.map(e => {
                        const titleElem = e.querySelector('.product-item__name h2, .product-item__name, h1, h2, h3, h4, a[title], [class*=""name""], [class*=""title""]');

                        let priceText = '';
                        const priceSelectors = [
                            '.product-item__new-price span',
                            '.product-item__new-price',
                            '.price',
                            '.preco',
                            '.amount',
                            '.value',
                            '[class*=""price""] span',
                            '[class*=""price""]',
                            'strong',
                            'span',
                            'div'
                        ];
                        for (const sel of priceSelectors) {
                            const el = e.querySelector(sel);
                            if (el && currencyRe.test(el.textContent)) { priceText = el.textContent.trim(); break; }
                        }
                        if (!priceText) {
                            const cand = Array.from(e.querySelectorAll('*')).find(el => currencyRe.test(el.textContent));
                            priceText = cand ? cand.textContent.trim() : '';
                        }

                        const imgElem = e.querySelector('img');
                        const linkElem = e.querySelector('a[href]');

                        const title = titleElem ? titleElem.textContent.trim() : (linkElem ? (linkElem.getAttribute('title') || linkElem.textContent.trim()) : 'Sem título');
                        const img = imgElem ? (imgElem.getAttribute('src') || '') : '';
                        const link = linkElem ? (linkElem.getAttribute('href') || '') : '';

                        return { title, priceText, link, img };
                    });
                ";
                var data = (IReadOnlyCollection<object>)js.ExecuteScript(script);
                #pragma warning restore CS8600

                Console.WriteLine($"📦 {data.Count} possíveis produtos encontrados.");

                var seen = new HashSet<string>();

                foreach (var obj in data)
                {
                    var dict = (Dictionary<string, object?>)obj;
                    string title = dict["title"]?.ToString()?.Trim() ?? "";
                    string link = dict["link"]?.ToString() ?? "";
                    string priceText = dict["priceText"]?.ToString() ?? "";
                    string img = dict["img"]?.ToString() ?? "";

                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link) || !priceText.Contains("R$"))
                        continue;

                    var match = Regex.Match(priceText, @"\d{1,3}(\.\d{3})*(,\d{2})?");
                    if (!match.Success) continue;
                    var normalized = match.Value.Replace(".", "").Replace(",", ".");
                    decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var price);

                    if (price <= 0) continue;

                    string key = $"{title}|{price}";
                    if (!seen.Add(key)) continue;

                    if (!link.StartsWith("http"))
                        link = new Uri(new Uri(url), link).ToString();

                    offers.Add(new OfferMessage
                    {
                        Title = title,
                        Price = price,
                        Url = link,
                        Store = ExtractDomain(url),
                        Category = "Geral",
                    });

                    Console.WriteLine($"✅ {title} - R${price}");
                }

                driver.Quit();
                Console.WriteLine($"🧹 Selenium remoto finalizado. Total de produtos: {offers.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO no GenericScraper: {ex.Message}");
            }

            return offers;
        }

        private static string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                return "Desconhecida";
            }
        }
    }
}
