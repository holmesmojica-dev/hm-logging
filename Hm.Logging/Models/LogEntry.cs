using System.ComponentModel.DataAnnotations;

namespace Hm.Logging.Models
{
    public class LogEntry
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
