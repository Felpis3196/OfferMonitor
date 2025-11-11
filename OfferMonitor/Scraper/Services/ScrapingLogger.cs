using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Scraper.Services
{
    public class ScrapingLogger : IDisposable
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _httpClient;
        private readonly string _requestId;
        private readonly Queue<string> _logQueue = new();
        private readonly Timer _flushTimer;

        public ScrapingLogger(string apiBaseUrl, string requestId)
        {
            _apiBaseUrl = apiBaseUrl.TrimEnd('/');
            _requestId = requestId;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            
            // Timer para enviar logs em batch a cada 500ms
            _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        public void Log(string message, string level = "INFO")
        {
            try
            {
                // Remove linhas vazias ou apenas espaços
                if (string.IsNullOrWhiteSpace(message))
                    return;

                // Tenta determinar o nível baseado no conteúdo da mensagem
                if (message.Contains("✅") || message.Contains("SUCCESS") || message.Contains("coletadas") || message.Contains("enviadas"))
                    level = "SUCCESS";
                else if (message.Contains("⚠️") || message.Contains("WARNING") || message.Contains("Nenhuma"))
                    level = "WARNING";
                else if (message.Contains("❌") || message.Contains("ERROR") || message.Contains("ERRO") || message.Contains("Erro"))
                    level = "ERROR";
                else if (message.Contains("🔍") || message.Contains("🚀") || message.Contains("📦") || message.Contains("🌐") || 
                         message.Contains("📥") || message.Contains("🔌") || message.Contains("⏳") || message.Contains("Conectando") ||
                         message.Contains("Acessando") || message.Contains("Aguardando"))
                    level = "INFO";

                // Adiciona à fila para envio em batch
                lock (_logQueue)
                {
                    _logQueue.Enqueue(message);
                }
            }
            catch
            {
                // Garante que erros de logging não quebrem o scraping
            }
        }

        private void FlushLogs(object? state)
        {
            List<string> logsToSend;
            lock (_logQueue)
            {
                if (_logQueue.Count == 0)
                    return;

                logsToSend = new List<string>();
                while (_logQueue.Count > 0 && logsToSend.Count < 10)
                {
                    logsToSend.Add(_logQueue.Dequeue());
                }
            }

            // Envia logs em batch
            _ = Task.Run(async () =>
            {
                foreach (var message in logsToSend)
                {
                    try
                    {
                        await SendLogToApi(message);
                    }
                    catch
                    {
                        // Silenciosamente ignora erros
                    }
                }
            });
        }

        private async Task SendLogToApi(string message)
        {
            try
            {
                string level = "INFO";
                if (message.Contains("✅") || message.Contains("SUCCESS"))
                    level = "SUCCESS";
                else if (message.Contains("⚠️") || message.Contains("WARNING"))
                    level = "WARNING";
                else if (message.Contains("❌") || message.Contains("ERROR") || message.Contains("ERRO"))
                    level = "ERROR";

                var logEntry = new
                {
                    requestId = _requestId,
                    message = message.Trim(),
                    level = level,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(logEntry);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/offers/scrape/logs",
                    content
                );
            }
            catch
            {
                // Silenciosamente ignora erros de envio de log
            }
        }

        public void Dispose()
        {
            // Envia logs restantes antes de dispor
            FlushLogs(null);
            Thread.Sleep(1000); // Aguarda envio dos últimos logs
            
            _flushTimer?.Dispose();
            _httpClient?.Dispose();
        }
    }
}

