using Application.Dto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Scraper.Helpers;
using System.Text;

namespace Scraper.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange = "offers_exchange";

        public RabbitMqPublisher(IConfiguration cfg)
        {
            _connection = RabbitMqHelper.GetConnectionWithRetry(
                cfg["RabbitMQ:Host"] ?? "localhost",
                cfg["RabbitMQ:Username"] ?? "guest",
                cfg["RabbitMQ:Password"] ?? "guest"
            );

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout, durable: true);
        }

        public void Publish(List<OfferInput> offers)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(offers));
            _channel.BasicPublish(exchange: _exchange, routingKey: "", basicProperties: null, body: body);
            Console.WriteLine($"📦 Publicadas {offers.Count} ofertas em '{_exchange}'");
        }
    }
}