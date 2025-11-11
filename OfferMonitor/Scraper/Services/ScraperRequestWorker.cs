using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scraper.Helpers;
using System.Text;
using System.Text.Json;

namespace Scraper.Services
{
    public class ScraperRequestWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IModel? _channel;

        public ScraperRequestWorker(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = RabbitMqHelper.GetConnectionWithRetry(
                _config["RabbitMQ:Host"] ?? "rabbitmq",
                _config["RabbitMQ:Username"] ?? "guest",
                _config["RabbitMQ:Password"] ?? "guest"
            );

            _channel = _connection.CreateModel();
            _channel.QueueDeclare("scrape_requests", durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                string requestId = string.Empty;
                string url = string.Empty;

                try
                {
                    // Tenta extrair RequestId dos headers
                    if (ea.BasicProperties.Headers != null && 
                        ea.BasicProperties.Headers.TryGetValue("RequestId", out var requestIdObj))
                    {
                        requestId = Encoding.UTF8.GetString((byte[])requestIdObj);
                    }

                    // Tenta deserializar mensagem JSON
                    var messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());
                    try
                    {
                        var message = JsonSerializer.Deserialize<JsonElement>(messageBody);
                        if (message.TryGetProperty("Url", out var urlProp))
                            url = urlProp.GetString() ?? messageBody;
                        if (message.TryGetProperty("RequestId", out var requestIdProp))
                            requestId = requestIdProp.GetString() ?? requestId;
                    }
                    catch
                    {
                        // Se não for JSON, assume que é apenas a URL
                        url = messageBody;
                    }

                    // Se não tem RequestId, gera um novo
                    if (string.IsNullOrEmpty(requestId))
                    {
                        requestId = Guid.NewGuid().ToString();
                    }

                    // Se não tem URL, usa o body como URL
                    if (string.IsNullOrEmpty(url))
                    {
                        url = messageBody;
                    }

                    var apiBaseUrl = _config["ApiBaseUrl"] ?? "http://api:8080";
                    var logger = new ScrapingLogger(apiBaseUrl, requestId);
                    
                    try
                    {
                        logger.Log($"📥 Pedido recebido: {url}", "INFO");

                        using var scope = _scopeFactory.CreateScope();
                        var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
                        await scraper.RunScraperAsync(url, logger);

                        logger.Log("✅ Processamento do scraping concluído com sucesso", "SUCCESS");
                    }
                    finally
                    {
                        // Aguarda um pouco para garantir que todos os logs sejam enviados
                        await Task.Delay(1000);
                        logger.Dispose();
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(requestId))
                    {
                        var apiBaseUrl = _config["ApiBaseUrl"] ?? "http://api:8080";
                        var logger = new ScrapingLogger(apiBaseUrl, requestId);
                        logger.Log($"❌ Erro ao processar scraping: {ex.Message}", "ERROR");
                        logger.Dispose();
                    }
                    Console.WriteLine($"❌ Erro ao processar mensagem: {ex.Message}");
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(queue: "scrape_requests", autoAck: false, consumer: consumer);
            Console.WriteLine("🔧 Aguardando mensagens em 'scrape_requests'...");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
