using RabbitMQ.Client;

namespace Scraper.Helpers
{
    public static class RabbitMqHelper
    {
        public static IConnection GetConnectionWithRetry(string host, string user, string pass, int maxRetries = 10, int delaySeconds = 5)
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass
            };

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var conn = factory.CreateConnection();
                    Console.WriteLine($"✅ Conectado ao RabbitMQ (tentativa {i + 1})");
                    return conn;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Falha ao conectar ao RabbitMQ (tentativa {i + 1}/{maxRetries}): {ex.Message}");
                    if (i == maxRetries - 1)
                        throw;
                    Thread.Sleep(delaySeconds * 1000);
                }
            }

            throw new Exception("❌ Não foi possível criar a conexão com o RabbitMQ.");
        }

        public static async Task<IConnection> GetConnectionWithRetryAsync(string host, string user, string pass, int maxRetries = 10, int delaySeconds = 5)
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass
            };

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var conn = factory.CreateConnection();
                    Console.WriteLine($"✅ Conectado ao RabbitMQ (tentativa {i + 1})");
                    return conn;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Tentativa {i + 1}/{maxRetries} falhou: {ex.Message}");
                    if (i == maxRetries - 1)
                        throw;
                    await Task.Delay(delaySeconds * 1000);
                }
            }

            throw new Exception("❌ Não foi possível criar a conexão com o RabbitMQ.");
        }
    }
}
