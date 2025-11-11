using Scraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace Scraper.Services.Implementations
{
    public class MercadoLivreScraper : ISiteScraper
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

                // Scroll progressivo
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

                await Task.Delay(2000);

                #pragma warning disable CS8600
                var script = @"
                    const cards = Array.from(document.querySelectorAll('div.andes-card.poly-card'));
                    return cards.map(card => {
                        const titleElem = card.querySelector('.poly-component__title');
                        const title = titleElem ? titleElem.innerText.trim() : 'Sem título';

                        // CAPTURA DE PREÇO
                        let priceText = '';

                        const priceElement = card.querySelector('.poly-price__current [role=""img""]') 
                                          || card.querySelector('.andes-money-amount[role=""img""]')
                                          || card.querySelector('.andes-money-amount__fraction');

                        if (priceElement) {
                            // Tenta capturar texto direto
                            priceText = priceElement.innerText || '';
                            // Se não encontrar texto, tenta pelo aria-label (ex: ""Agora: 72 reais com 92 centavos"")
                            if (!priceText.trim() && priceElement.getAttribute('aria-label')) {
                                priceText = priceElement.getAttribute('aria-label');
                            }
                        }
                        priceText = priceText
                          .replace(/[^\d,]/g, '')
                          .replace(',', '.');

                        const linkElem = card.querySelector('a.poly-component__title');
                        const link = linkElem ? linkElem.href : '';

                        const brandElem = card.querySelector('.poly-component__brand');
                        const brand = brandElem ? brandElem.innerText.trim() : '';

                        const ratingElem = card.querySelector('.poly-reviews__rating');
                        const rating = ratingElem ? ratingElem.innerText.trim() : '';

                        const discountElem = card.querySelector('.andes-money-amount__discount');
                        const discount = discountElem ? discountElem.innerText.trim() : '';

                        return { title, price: priceText, link, brand, rating, discount };
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
                    if (!seen.Add(key)) continue;

                    var brand = dict["brand"]?.ToString() ?? "";
                    var rating = dict["rating"]?.ToString() ?? "";
                    var discount = dict["discount"]?.ToString() ?? "";

                    offers.Add(new OfferMessage
                    {
                        Title = title,
                        Price = price,
                        Url = link,
                        Store = "Mercado Livre",
                        Category = brand,
                        Discount = !string.IsNullOrEmpty(discount) ? discount : $"{rating}★"
                    });

                    Console.WriteLine($"✅ {title} - R${price:F2} ({brand}) {discount} ★{rating}");
                }

                driver.Quit();
                Console.WriteLine("🧹 Selenium remoto finalizado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO no Selenium remoto: {ex.Message}");
            }

            Console.WriteLine($"🔎 Total de produtos válidos: {offers.Count}");
            return offers;
        }
    }
}
