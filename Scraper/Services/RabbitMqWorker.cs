using Application.Dto;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scraper.Helpers;
using System.Text;

namespace Scraper.Services
{
    public class RabbitMqWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqWorker(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = RabbitMqHelper.GetConnectionWithRetry(
                _config["RabbitMQ:Host"] ?? "localhost",
                _config["RabbitMQ:Username"] ?? "guest",
                _config["RabbitMQ:Password"] ?? "guest"
            );

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("offers_exchange", ExchangeType.Fanout, durable: true);

            var queue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue, "offers_exchange", "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessage;

            _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            Console.WriteLine("🐇 Escutando mensagens em 'offers_exchange'...");
            return Task.CompletedTask;
        }

        private async void OnMessage(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var offers = JsonConvert.DeserializeObject<List<OfferInput>>(Encoding.UTF8.GetString(e.Body.ToArray()));
                if (offers == null) return;

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IOfferRepository>();

                foreach (var o in offers)
                {
                    var uri = new Uri(o.Url);
                    await repo.AddAsync(new Offer
                    {
                        Title = o.Title,
                        Store = o.Store,
                        Category = o.Category,
                        Url = o.Url,
                        Price = o.Price,
                        Domain = uri.Host.Replace("www.", "")
                    });
                }

                _channel!.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar mensagem: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
