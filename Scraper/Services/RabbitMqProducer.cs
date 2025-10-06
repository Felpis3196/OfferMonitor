using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Scraper.Services;

public class RabbitMqConsumer
{
    private readonly string _queueName = "offers_queue";

    public void Consume()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        using var connection = factory.CreateConnection("offers-monitor-connection");
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
        };

        channel.BasicConsume(
            queue: _queueName,
            autoAck: true,
            consumer: consumer);

        Console.WriteLine(" [*] Waiting for messages. Press [enter] to exit.");
        Console.ReadLine();
    }
}
