using Hm.Logging.Enums;

namespace Hm.Logging.Configuration
{
    /// <summary>
    /// Represents configuration settings for the logging pipeline.
    /// </summary>
    public sealed class LoggingOptions
    {
        /// <summary>
        /// Gets or sets the minimum log level allowed for processing.
        /// Logs below this level will be ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
    }
}
