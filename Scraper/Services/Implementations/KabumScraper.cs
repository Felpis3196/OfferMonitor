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
                // Tenta via API pública
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
                        Category = "Eletrônicos"
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

            Console.WriteLine("Iniciando ChromeDriver...");

            try
            {
                var options = new ChromeOptions();

                // Configurações mínimas e eficientes
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");

                // Configuração simplificada do serviço
                using var driver = new ChromeDriver(options);

                Console.WriteLine("ChromeDriver iniciado com sucesso!");

                driver.Navigate().GoToUrl(url);
                Console.WriteLine($"Página carregada: {url}");

                // Aguarda elementos carregarem
                await Task.Delay(3000);

                var products = driver.FindElements(By.CssSelector("div.productCard"));
                Console.WriteLine($"Encontrados {products.Count} produtos");

                foreach (var product in products.Take(5)) // Limita para testes
                {
                    try
                    {
                        var title = product.FindElement(By.CssSelector(".nameCard"))?.Text?.Trim() ?? "Sem título";
                        var priceText = product.FindElement(By.CssSelector(".priceCard"))?.Text?.Trim() ?? "0";

                        // Parsing seguro do preço
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
                            Category = "Eletrônicos"
                        });

                        Console.WriteLine($"Produto extraído: {title} - R$ {price}");
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
