namespace Hm.Logging.Models
{
    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public LogLevel Level { get; set; } = LogLevel.Information;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
