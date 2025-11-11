using System.Collections.Concurrent;

namespace Application.Services
{
    public class ScrapingLogEntry
    {
        public string RequestId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "INFO"; // INFO, SUCCESS, WARNING, ERROR
    }

    public interface IScrapingLogService
    {
        void AddLog(string requestId, string message, string level = "INFO");
        List<ScrapingLogEntry> GetLogs(string key);
        void ClearLogs(string requestId);
        bool HasLogs(string requestId);
    }

    public class ScrapingLogService : IScrapingLogService
    {
        private readonly ConcurrentDictionary<string, List<ScrapingLogEntry>> _logs = new();
        private readonly object _lock = new object();

        public void AddLog(string requestId, string message, string level = "INFO")
        {
            var entry = new ScrapingLogEntry
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Message = message,
                Level = level
            };

            _logs.AddOrUpdate(
                requestId,
                new List<ScrapingLogEntry> { entry },
                (key, existing) =>
                {
                    lock (_lock)
                    {
                        existing.Add(entry);
                        return existing;
                    }
                }
            );
        }

        public List<ScrapingLogEntry> GetLogs(string key = "global")
        {
            if (_logs.TryGetValue(key, out var logs))
                return logs.OrderBy(l => l.Timestamp).ToList();

            return new List<ScrapingLogEntry>();
        }

        public void ClearLogs(string requestId)
        {
            _logs.TryRemove(requestId, out _);
        }

        public bool HasLogs(string requestId)
        {
            return _logs.ContainsKey(requestId);
        }
    }
}

