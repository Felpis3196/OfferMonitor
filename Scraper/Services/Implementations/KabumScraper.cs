using Scraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Linq;

namespace Scraper.Services.Implementations
{
    public class KabumScraper : ISiteScraper
    {
        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();
            var seleniumUrl = Environment.GetEnvironmentVariable("SELENIUM_URL")
                              ?? "http://selenium:4444/wd/hub";

            Console.WriteLine($"🚀 Conectando ao Selenium remoto: {seleniumUrl}");

            try
            {
                var options = new ChromeOptions();
                options.AddArguments(new string[]
                {
                    "--headless=new",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--window-size=1920,1080",
                    "--disable-extensions",
                    "--disable-blink-features=AutomationControlled",
                    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0 Safari/537.36"
                });
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                using var driver = new RemoteWebDriver(
                    new Uri(seleniumUrl),
                    options.ToCapabilities(),
                    TimeSpan.FromSeconds(90));

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);

                Console.WriteLine($"🌐 Acessando: {url}");
                driver.Navigate().GoToUrl(url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

                // tenta fechar pop-up de cookies
                try
                {
                    var cookieBtn = driver.FindElements(By.CssSelector("button#onetrust-accept-btn-handler")).FirstOrDefault();
                    cookieBtn?.Click();
                }
                catch { }

                // tenta localizar os produtos principais
                try
                {
                    wait.Until(d => d.FindElements(By.CssSelector("div.productCard, div.productCardV2")).Any());
                }
                catch
                {
                    Console.WriteLine("⚠️ Nenhum seletor 'productCard' encontrado — aplicando fallback baseado em R$...");
                    // fallback: aguarda qualquer elemento que contenha 'R$' no texto
                    wait.Until(d =>
                        d.FindElements(By.XPath("//*[contains(text(),'R$')]")).Any());
                }

                var products = driver.FindElements(By.CssSelector("div.productCard, div.productCardV2"));
                if (!products.Any())
                {
                    // fallback manual — busca qualquer coisa com preço
                    products = driver.FindElements(By.XPath("//*[contains(text(),'R$')]"));
                }

                Console.WriteLine($"📦 {products.Count} produtos encontrados (após fallback).");

                foreach (var product in products)
                {
                    try
                    {
                        var title = "Sem título";
                        try
                        {
                            title = product.FindElement(By.CssSelector(".nameCard"))?.Text?.Trim() ?? "Sem título";
                        }
                        catch
                        {
                            // tenta capturar texto próximo
                            title = product.Text?.Split('\n').FirstOrDefault(t => !t.Contains("R$")) ?? "Sem título";
                        }

                        var priceText = "";
                        try
                        {
                            priceText = product.FindElement(By.CssSelector(".priceCard"))?.Text?.Trim() ?? "";
                        }
                        catch
                        {
                            // fallback: busca dentro do próprio texto
                            priceText = product.Text?.Split('\n').FirstOrDefault(t => t.Contains("R$")) ?? "";
                        }

                        if (string.IsNullOrEmpty(priceText) || !priceText.Contains("R$"))
                            continue;

                        var cleanPrice = priceText
                            .Replace("R$", "")
                            .Replace(".", "")
                            .Replace(",", ".")
                            .Trim();

                        if (!decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price <= 0)
                            continue;

                        string link = url;
                        try
                        {
                            link = product.FindElement(By.TagName("a"))?.GetAttribute("href") ?? url;
                        }
                        catch { }

                        offers.Add(new OfferMessage
                        {
                            Title = title,
                            Price = price,
                            Url = link,
                            Store = "Kabum",
                            Category = "Eletrônicos"
                        });

                        Console.WriteLine($"✅ Produto extraído: {title} — R$ {price}");
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"⚠️ Erro ao extrair produto: {innerEx.Message}");
                    }
                }

                driver.Quit();
                Console.WriteLine("🧹 Selenium remoto finalizado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO no Selenium remoto: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine($"🔎 Total de produtos válidos encontrados: {offers.Count}");
            return offers;
        }
    }
}
