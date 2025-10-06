using Application.Dto;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Scraper.Services
{
    public class RabbitMqWorker : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqWorker(IServiceScopeFactory scopeFactory, IConfiguration cfg)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory()
            {
                HostName = cfg["RabbitMQ:Host"],
                UserName = cfg["RabbitMQ:User"] ?? "guest",
                Password = cfg["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("offers", ExchangeType.Fanout);
            var queue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue, "offers", "");
            _channel.BasicQos(0, 10, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnReceived;
            _channel.BasicConsume(queue, false, consumer);
        }

        private async void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var json = Encoding.UTF8.GetString(e.Body.ToArray());
                var offers = JsonConvert.DeserializeObject<List<OfferInput>>(json);

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IOfferRepository>();

                if (offers != null)
                {
                    foreach (var offerInput in offers)
                    {
                        // 🔹 Extrai domínio do URL
                        var uri = new Uri(offerInput.Url);
                        var domain = uri.Host.Replace("www.", "");

                        var offer = new Offer
                        {
                            Title = offerInput.Title,
                            Store = offerInput.Store,
                            Category = offerInput.Category,
                            Url = offerInput.Url,
                            Price = offerInput.Price,
                            Domain = domain
                        };

                        await repo.AddAsync(offer);
                    }
                }

                _channel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar mensagem do RabbitMQ: {ex.Message}");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Worker roda em background continuamente
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
