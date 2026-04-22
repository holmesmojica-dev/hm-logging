namespace Hm.Logging.Models
{
    internal sealed class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
