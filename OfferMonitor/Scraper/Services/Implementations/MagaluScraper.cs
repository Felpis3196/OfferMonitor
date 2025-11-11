using Scraper.Models;
using Scraper.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace Scraper.Services.Implementations
{
    public class MagaluScraper : ISiteScraper
    {
        private static readonly HttpClient _httpClient = new();

        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();

            var seleniumUrl = Environment.GetEnvironmentVariable("SELENIUM_REMOTE_URL")
                ?? "http://selenium:4444/wd/hub";

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--window-size=1920,1080",
                "--ignore-certificate-errors",
                "--disable-blink-features=AutomationControlled",
                "--disable-extensions",
                "--headless=new",
                "--lang=pt-BR",
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            );

            using var driver = new RemoteWebDriver(new Uri(seleniumUrl), chromeOptions);
            driver.Navigate().GoToUrl(url);

            var js = (IJavaScriptExecutor)driver;

            // Força scroll para carregar produtos dinâmicos
            for (int i = 0; i < 10; i++)
            {
                js.ExecuteScript("window.scrollBy(0, document.body.scrollHeight)");
                await Task.Delay(1300);
            }

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(drv => ((IJavaScriptExecutor)drv)
                    .ExecuteScript("return document.querySelectorAll('a[data-testid=\"product-card-container\"]').length > 0;"));
            }
            catch
            {
                LoggingHelper.Log("⚠️ Nenhum produto visível via Selenium. Tentando via API...", "WARNING");
            }

            // --- SCRIPT JAVASCRIPT ROBUSTO ---
            var script = @"
                const cards = document.querySelectorAll('a[data-testid=""product-card-container""]');
                return Array.from(cards).map(card => {
                    const titleElem = card.querySelector('[data-testid=""product-title""]');
                    const priceNowElem = card.querySelector('[data-testid=""price-value""]');
                    const priceOldElem = card.querySelector('[data-testid=""price-original""]');
                    const imgElem = card.querySelector('img[data-testid=""image""]');
                    const linkElem = card.tagName === 'A' ? card : card.closest('a');
                    const brand = card.getAttribute('data-brand') || '';

                    let priceText = '';
                    if (priceNowElem) {
                        priceText = priceNowElem.textContent.replace(/\s+/g, ' ').trim();
                        const match = priceText.match(/R\$\s*[\d\.\,]+/);
                        priceText = match ? match[0] : priceText;
                    } else if (priceOldElem) {
                        let t = priceOldElem.textContent.replace(/\s+/g, ' ').trim();
                        const m = t.match(/R\$\s*[\d\.\,]+/);
                        priceText = m ? m[0] : t;
                    }

                    return {
                        title: titleElem ? titleElem.textContent.trim() : '',
                        price: priceText,
                        link: linkElem ? linkElem.href : '',
                        image: imgElem ? imgElem.src : '',
                        brand: brand
                    };
                });
            ";

            var results = (ReadOnlyCollection<object>)js.ExecuteScript(script);

            if (results.Count > 0)
            {
                foreach (var r in results)
                {
                    if (r is not Dictionary<string, object> dict)
                        continue;

                    var title = dict.GetValueOrDefault("title")?.ToString() ?? "";
                    var priceStr = dict.GetValueOrDefault("price")?.ToString() ?? "";
                    var link = dict.GetValueOrDefault("link")?.ToString() ?? "";
                    var brand = dict.GetValueOrDefault("brand")?.ToString() ?? "";
                    var image = dict.GetValueOrDefault("image")?.ToString() ?? "";

                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(priceStr))
                    {
                        if (decimal.TryParse(
                            priceStr.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out decimal price))
                        {
                            if (!string.IsNullOrWhiteSpace(link) && link.StartsWith("/"))
                            {
                                link = "https://www.magazineluiza.com.br" + link;
                            }
                            offers.Add(new OfferMessage
                            {
                                Title = title,
                                Price = price,
                                Url = link,
                                Store = "Magalu",
                                Category = brand
                            });

                            LoggingHelper.Log($"✅ Capturado: {title} - R${price}", "SUCCESS");
                        }
                    }
                }
            }

            // --- Fallback: API Magalu ---
            if (offers.Count == 0)
            {
                try
                {
                    var searchTerm = ExtractSearchTerm(url);
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        var apiUrl = $"https://www.magazineluiza.com.br/busca/api/v2/{searchTerm}/";
                        LoggingHelper.Log($"🔍 Buscando via API: {apiUrl}", "INFO");

                        var response = await _httpClient.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var doc = JsonDocument.Parse(json);

                            var items = doc.RootElement.GetProperty("data").EnumerateArray();
                            foreach (var item in items)
                            {
                                var title = item.GetProperty("title").GetString() ?? "";
                                var link = "https://www.magazineluiza.com.br" + (item.GetProperty("url").GetString() ?? "");
                                var price = item.TryGetProperty("price", out var p) ? p.GetDecimal() : 0;

                                if (!string.IsNullOrEmpty(title) && price > 0)
                                {
                                    offers.Add(new OfferMessage
                                    {
                                        Title = title,
                                        Price = price,
                                        Url = link,
                                        Store = "Magalu (API)"
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao buscar via API: {ex.Message}");
                }
            }

            Console.WriteLine($"🏁 Total de ofertas encontradas (Magalu): {offers.Count}");
            return offers;
        }

        private string ExtractSearchTerm(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 1 && segments[0].Equals("busca", StringComparison.OrdinalIgnoreCase))
                    return segments[1];
            }
            catch { }
            return "";
        }
    }
}
