using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scraper.Helpers;
using System.Text;

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
                var url = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"📥 Pedido recebido: {url}");

                using var scope = _scopeFactory.CreateScope();
                var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
                await scraper.RunScraperAsync(url);

                _channel.BasicAck(ea.DeliveryTag, false);
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
