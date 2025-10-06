using Application.Dto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Scraper.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public RabbitMqPublisher(IConfiguration cfg)
        {
            var factory = new ConnectionFactory()
            {
                HostName = cfg["RabbitMQ:Host"],
                UserName = cfg["RabbitMQ:Username"],
                Password = cfg["RabbitMQ:Password"]
            };

            const int maxRetries = 10;
            const int delaySeconds = 5;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    Console.WriteLine($"Conectado ao RabbitMQ na tentativa {i + 1}");
                    _exchange = "offers_exchange";
                    _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout, durable: true);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Tentativa {i + 1}/{maxRetries} falhou: {ex.Message}");
                    if (i == maxRetries - 1) throw;
                    Thread.Sleep(delaySeconds * 1000);
                }
            }
        }

        public void Publish(List<OfferInput> offers)
        {
            var json = JsonConvert.SerializeObject(offers);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: _exchange,
                                  routingKey: "",
                                  basicProperties: null,
                                  body: body);

            Console.WriteLine($" [x] Publicado {offers.Count} ofertas");
        }
    }
}
