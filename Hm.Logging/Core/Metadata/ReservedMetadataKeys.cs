namespace Hm.Logging.Core.Metadata
{
    /// <summary>
    /// Defines reserved metadata keys used internally by the logging pipeline.
    /// </summary>
    /// <remarks>
    /// These keys represent core logging properties and must not be overridden
    /// through custom metadata entries.
    ///
    /// <para>
    /// Reserved keys are protected to preserve consistency, traceability,
    /// and provider compatibility across the logging pipeline.
    /// </para>
    ///
    /// <para>
    /// Attempting to use any reserved key as custom metadata will result
    /// in validation failure during metadata normalization.
    /// </para>
    /// </remarks>
    internal static class ReservedMetadataKeys
    {
        public static IReadOnlySet<string> Keys { get; } =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "TraceId",
            "Timestamp",
            "Level",
            "Message",
            "Source"
        };
    }
}
