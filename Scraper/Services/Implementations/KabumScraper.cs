using Scraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net.Http.Json;
using OpenQA.Selenium.Support.UI;

namespace Scraper.Services.Implementations
{
    public class KabumScraper : ISiteScraper
    {
        private readonly HttpClient _http = new();

        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            try
            {
                // Tenta via API publica
                var apiUrl = "https://servicespub.prod.api.aws.grupokabum.com.br/descricao/v1/departamentos/hardware/categorias/placa-de-video/produtos";
                var json = await _http.GetFromJsonAsync<KabumResponse>(apiUrl);

                if (json?.Produtos != null && json.Produtos.Count > 0)
                {
                    return json.Produtos.Select(p => new OfferMessage
                    {
                        Title = p.Nome,
                        Price = p.Preco,
                        Url = $"https://www.kabum.com.br/produto/{p.Id}",
                        Store = "Kabum",
                        Category = "Eletronicos"
                    }).ToList();
                }
            }
            catch
            {
                // Ignora erro e tenta Selenium
            }

            // Fallback Selenium
            return await ScrapeWithSelenium(url);
        }
        private async Task<List<OfferMessage>> ScrapeWithSelenium(string url)
        {
            var offers = new List<OfferMessage>();

            Console.WriteLine("Iniciando ChromeDriver no container...");

            try
            {
                var options = new ChromeOptions();

                // Configura��es otimizadas para container Docker
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-plugins");
                options.AddArgument("--disable-images");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddArgument("--disable-javascript");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                // Configura��es de performance
                options.AddArgument("--memory-pressure-off");
                options.AddArgument("--max_old_space_size=2048");

                using var driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(60));

                Console.WriteLine("ChromeDriver iniciado com sucesso no container!");

                // Timeout configurado
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                driver.Navigate().GoToUrl(url);
                Console.WriteLine($"P�gina carregada: {url}");

                // Aguarda elementos carregarem
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.CssSelector("div.productCard")));

                var products = driver.FindElements(By.CssSelector("div.productCard"));
                Console.WriteLine($"Encontrados {products.Count} produtos");

                foreach (var product in products.Take(5))
                {
                    try
                    {
                        var title = product.FindElement(By.CssSelector(".nameCard"))?.Text?.Trim() ?? "Sem t�tulo";
                        var priceText = product.FindElement(By.CssSelector(".priceCard"))?.Text?.Trim() ?? "0";

                        var cleanPrice = priceText.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim();
                        decimal price = decimal.TryParse(cleanPrice,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var result) ? result : 0;

                        var link = product.FindElement(By.TagName("a"))?.GetAttribute("href") ?? url;

                        offers.Add(new OfferMessage
                        {
                            Title = title,
                            Price = price,
                            Url = link,
                            Store = "Kabum",
                            Category = "Eletr�nicos"
                        });

                        Console.WriteLine($"Produto extra�do: {title} - R$ {price}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao extrair produto: {ex.Message}");
                        continue;
                    }
                }

                driver.Quit();
                Console.WriteLine("ChromeDriver finalizado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no Selenium: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return offers;
        }

        private class KabumResponse
        {
            public List<KabumProduct> Produtos { get; set; } = new();
        }

        private class KabumProduct
        {
            public int Id { get; set; }
            public string Nome { get; set; } = "";
            public decimal Preco { get; set; }
        }
    }
}
