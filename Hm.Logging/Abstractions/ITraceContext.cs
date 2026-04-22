namespace Hm.Logging.Abstractions
{
    /// <summary>
    /// Provides access to the current trace context for correlation purposes.
    /// </summary>
    public interface ITraceContext
    {
        /// <summary>
        /// Gets the current trace identifier.
        /// </summary>
        /// <returns>The trace identifier if available; otherwise null.</returns>
        string? GetTraceId();
    }
}
