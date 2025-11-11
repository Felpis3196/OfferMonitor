namespace Scraper.Services
{
    public static class LoggingHelper
    {
        private static ScrapingLogger? _currentLogger;

        public static void SetLogger(ScrapingLogger? logger)
        {
            _currentLogger = logger;
        }

        public static void Log(string message, string level = "INFO")
        {
            _currentLogger?.Log(message, level);
            Console.WriteLine(message);
        }
    }
}

