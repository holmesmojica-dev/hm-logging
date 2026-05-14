using Hm.Logging.Enums;

namespace Hm.Logging.Configuration;

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

    /// <summary>
    /// Gets or sets the maximum allowed length for log messages.
    /// </summary>
    /// <remarks>
    /// Messages exceeding this limit will cause validation failure
    /// before being processed or dispatched by the logging pipeline.
    ///
    /// <para>
    /// A value of <c>0</c> disables message length validation,
    /// allowing messages of any size.
    /// </para>
    ///
    /// <para>
    /// The default value is <c>4000</c>.
    /// </para>
    /// </remarks>
    public uint MaxMessageLength { get; set; } = 4000;
}
