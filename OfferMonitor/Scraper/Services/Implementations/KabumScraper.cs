using Scraper.Models;
using Scraper.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace Scraper.Services.Implementations
{
    public class KabumScraper : ISiteScraper
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
                options.AddArguments(new string[]
                {
                    "--headless=new",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-extensions",
                    "--disable-blink-features=AutomationControlled",
                    "--blink-settings=imagesEnabled=false",
                    "--window-size=1920,1080",
                    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0 Safari/537.36"
                });
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                using var driver = new RemoteWebDriver(
                    new Uri(seleniumUrl),
                    options.ToCapabilities(),
                    TimeSpan.FromSeconds(60));

                // remove implicit wait (muito lento)
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

                LoggingHelper.Log($"🌐 Acessando: {url}", "INFO");
                driver.Navigate().GoToUrl(url);

                // Espera só até produtos com "R$" aparecerem
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                wait.Until(d => d.FindElements(By.XPath("//*[contains(text(),'R$')]")).Any());

                // Captura dados diretamente via JS — muito mais rápido que Selenium loop
                var js = (IJavaScriptExecutor)driver;
                #pragma warning disable CS8600 // Conversão de literal nula ou possível valor nulo em tipo não anulável.
                var data = (IReadOnlyCollection<object>)js.ExecuteScript(@"
                    return Array.from(document.querySelectorAll('article.productCard'))
                        .filter(e => e.innerText.includes('R$'))
                        .map(e => {
                            const title = e.querySelector('a.productLink img')?.title 
                                || Array.from(e.innerText.split('\n')).find(t => t.length > 8 && !t.includes('R$')) 
                                || 'Sem título';
                            const priceMatch = e.innerText.match(/R\$\s?[\d.,]+/);
                            const price = priceMatch ? priceMatch[0].replace(/[^\d,]/g, '').replace(',', '.') : null;
                            const oldMatch = e.innerText.match(/De\s*R\$\s?[\d.,]+/);
                            const oldPrice = oldMatch ? oldMatch[0].replace(/[^\d,]/g, '').replace(',', '.') : null;
                            const discount = e.querySelector('.productCard__badge, .discountPercent')?.innerText || '';
                            const link = e.querySelector('a.productLink')?.href || '';
                            return { title, price, oldPrice, discount, link };
                        });
                ");
                #pragma warning restore CS8600 // Conversão de literal nula ou possível valor nulo em tipo não anulável.

                LoggingHelper.Log($"📦 {data.Count} produtos capturados via JS.", "INFO");

                foreach (var obj in data)
                {
                    var dict = (Dictionary<string, object?>)obj;
                    string title = dict["title"]?.ToString()?.Trim() ?? "Sem título";
                    string link = dict["link"]?.ToString() ?? "";

                    if (string.IsNullOrEmpty(link)) continue;

                    decimal.TryParse(dict["price"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
                    if (price <= 0) continue;

                    decimal? oldPrice = null;
                    if (decimal.TryParse(dict["oldPrice"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedOld))
                        oldPrice = parsedOld;

                    var discount = dict["discount"]?.ToString() ?? "";
                    offers.Add(new OfferMessage
                    {
                        Title = title,
                        Price = price,
                        OldPrice = oldPrice,
                        Discount = dict["discount"]?.ToString() ?? "",
                        Url = link.StartsWith("http") ? link : $"https://www.kabum.com.br{link}",
                        Store = "Kabum",
                        Category = "Eletrônicos"
                    });
                    LoggingHelper.Log($"{title} de {oldPrice} por {price} com desconto de {discount}", "SUCCESS");
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
