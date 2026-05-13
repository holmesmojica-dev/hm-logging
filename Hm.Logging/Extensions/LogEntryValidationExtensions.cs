using Hm.Logging.Abstractions;
using Hm.Logging.Core.Metadata;
using Hm.Logging.Models;

namespace Hm.Logging.Extensions
{
    /// <summary>
    /// Provides validation and normalization logic for <see cref="LogEntry"/> instances.
    /// </summary>
    /// <remarks>
    /// This class ensures that all log entries meet the required standards before being processed
    /// by the logging pipeline.
    ///
    /// <para>
    /// The validation process includes:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>Ensuring required fields such as <see cref="LogEntry.Message"/> are present.</description>
    /// </item>
    /// <item>
    /// <description>Normalizing string values (trimming and converting empty values to null).</description>
    /// </item>
    /// <item>
    /// <description>Assigning default values (e.g., UTC timestamp when not provided).</description>
    /// </item>
    /// <item>
    /// <description>Guaranteeing a valid <see cref="LogEntry.TraceId"/> for traceability.</description>
    /// </item>
    /// <item>
    /// <description>Cleaning and normalizing metadata while preserving original data types.</description>
    /// </item>
    /// </list>
    ///
    /// <para>
    /// This method acts as a safeguard to ensure consistency and reliability of logs,
    /// regardless of how they were created.
    /// </para>
    ///
    /// <para>
    /// It is intended to be used internally by the logging pipeline (e.g., <c>LoggerService</c>)
    /// and should be invoked before dispatching log entries to logging providers.
    /// </para>
    /// </remarks>
    public static class LogEntryValidationExtensions
    {
        /// <summary>
        /// Validates and normalizes the specified <see cref="LogEntry"/> instance.
        /// </summary>
        /// <param name="logEntry">The log entry to validate and normalize.</param>
        /// <param name="traceContext">
        /// Provides access to the current trace identifier used for distributed tracing.
        /// </param>
        /// <returns>
        /// A new <see cref="LogEntry"/> instance with normalized and validated values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logEntry"/> or <paramref name="traceContext"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="LogEntry.Message"/> is null, empty, or whitespace.
        /// </exception>
        /// <remarks>
        /// This method guarantees that the resulting log entry:
        ///
        /// <list type="bullet">
        /// <item>
        /// <description>Contains a non-empty message.</description>
        /// </item>
        /// <item>
        /// <description>Uses a valid UTC timestamp.</description>
        /// </item>
        /// <item>
        /// <description>Has normalized string properties (trimmed and cleaned).</description>
        /// </item>
        /// <item>
        /// <description>Includes a valid <see cref="LogEntry.TraceId"/>, either provided,
        /// retrieved from <paramref name="traceContext"/>, or generated if missing.</description>
        /// </item>
        /// <item>
        /// <description>Contains cleaned metadata with valid keys and values.</description>
        /// </item>
        /// </list>
        ///
        /// <para>
        /// This method does not mutate the original instance. Instead, it returns a new immutable
        /// instance using record cloning semantics.
        /// </para>
        /// </remarks>
        public static LogEntry EnsureValid(this LogEntry logEntry, ITraceContext traceContext)
        {
            ArgumentNullException.ThrowIfNull(logEntry);
            ArgumentNullException.ThrowIfNull(traceContext);
            ArgumentException.ThrowIfNullOrWhiteSpace(logEntry.Message);

            LogEntry normalizedLogEntry = logEntry with
            {
                Message = logEntry.Message.Trim(),
                Timestamp = logEntry.Timestamp == default ? DateTime.UtcNow : logEntry.Timestamp,

                Source = Normalize(logEntry.Source),
                TraceId = Normalize(logEntry.TraceId)
                        ?? traceContext.GetTraceId()
                        ?? Guid.NewGuid().ToString(),

                Exception = Normalize(logEntry.Exception),
                Metadata = logEntry.Metadata.EnsureValidMetadata()
            };

            return normalizedLogEntry;
        }


        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
