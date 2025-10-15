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
        //public async Task<List<OfferMessage>> ScrapeAsync(string url)
        //{
        //    var offers = new List<OfferMessage>();

        //    Console.WriteLine("Iniciando ChromeDriver no container...");

        //    try
        //    {
        //        var options = new ChromeOptions();

        //        // Configura��es otimizadas para container Docker
        //        options.AddArgument("--headless=new");
        //        options.AddArgument("--no-sandbox");
        //        options.AddArgument("--disable-dev-shm-usage");
        //        options.AddArgument("--disable-gpu");
        //        options.AddArgument("--window-size=1920,1080");
        //        options.AddArgument("--disable-extensions");
        //        options.AddArgument("--disable-plugins");
        //        options.AddArgument("--disable-images");
        //        options.AddArgument("--blink-settings=imagesEnabled=false");
        //        options.AddArgument("--disable-blink-features=AutomationControlled");
        //        options.AddExcludedArgument("enable-automation");
        //        options.AddAdditionalOption("useAutomationExtension", false);

        //        // Configura��es de performance
        //        options.AddArgument("--memory-pressure-off");
        //        options.AddArgument("--max_old_space_size=2048");

        //        using var driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(60));

        //        Console.WriteLine("ChromeDriver iniciado com sucesso no container!");

        //        // Timeout configurado
        //        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        //        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        //        driver.Navigate().GoToUrl(url);
        //        Console.WriteLine($"P�gina carregada: {url}");

        //        // Aguarda elementos carregarem
        //        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        //        wait.Until(d => d.FindElement(By.CssSelector("div.productCard")));

        //        var products = driver.FindElements(By.CssSelector("div.productCard"));
        //        Console.WriteLine($"Encontrados {products.Count} produtos");

        //        foreach (var product in products.Take(5))
        //        {
        //            try
        //            {
        //                var title = product.FindElement(By.CssSelector(".nameCard"))?.Text?.Trim() ?? "Sem t�tulo";
        //                var priceText = product.FindElement(By.CssSelector(".priceCard"))?.Text?.Trim() ?? "0";

        //                var cleanPrice = priceText.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim();
        //                decimal price = decimal.TryParse(cleanPrice,
        //                    System.Globalization.NumberStyles.Any,
        //                    System.Globalization.CultureInfo.InvariantCulture,
        //                    out var result) ? result : 0;

        //                var link = product.FindElement(By.TagName("a"))?.GetAttribute("href") ?? url;

        //                offers.Add(new OfferMessage
        //                {
        //                    Title = title,
        //                    Price = price,
        //                    Url = link,
        //                    Store = "Kabum",
        //                    Category = "Eletr�nicos"
        //                });

        //                Console.WriteLine($"Produto extra�do: {title} - R$ {price}");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Erro ao extrair produto: {ex.Message}");
        //                continue;
        //            }
        //        }

        //        driver.Quit();
        //        Console.WriteLine("ChromeDriver finalizado.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"ERRO no Selenium: {ex.Message}");
        //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        //    }

        //    return offers;
        //}

        public async Task<List<OfferMessage>> ScrapeAsync(string url)
        {
            var offers = new List<OfferMessage>();

            Console.WriteLine("Iniciando teste do Selenium...");

            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");

                using var driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(60));

                driver.Navigate().GoToUrl("https://www.kabum.com.br");
                Console.WriteLine("Página carregada!");

                Console.WriteLine("Título da página: " + driver.Title);

                var body = driver.FindElement(By.TagName("body"));
                Console.WriteLine("Body encontrado? " + (body != null));

                driver.Quit();
                Console.WriteLine("ChromeDriver finalizado com sucesso!");
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
