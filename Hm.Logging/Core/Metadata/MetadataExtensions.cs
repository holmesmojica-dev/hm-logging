using System.Collections.Immutable;

namespace Hm.Logging.Core.Metadata
{
    /// <summary>
    /// Provides validation and normalization utilities for structured log metadata.
    /// </summary>
    /// <remarks>
    /// This class centralizes metadata processing rules used throughout the logging pipeline.
    ///
    /// <para>
    /// The validation process includes:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>
    /// <description>Normalizing metadata keys and string values.</description>
    /// </item>
    /// <item>
    /// <description>Removing invalid or empty entries.</description>
    /// </item>
    /// <item>
    /// <description>Preventing usage of reserved metadata keys.</description>
    /// </item>
    /// <item>
    /// <description>Preserving immutable metadata behavior.</description>
    /// </item>
    /// <item>
    /// <description>Preparing metadata for future serialization and provider integrations.</description>
    /// </item>
    /// </list>
    ///
    /// <para>
    /// The implementation is intentionally lightweight and provider-agnostic
    /// to maintain compatibility with different logging targets such as files,
    /// databases, gRPC transports, and observability platforms.
    /// </para>
    /// </remarks>
    internal static class MetadataExtensions
    {
        /// <summary>
        /// Validates and normalizes structured log metadata.
        /// </summary>
        /// <param name="metadata">
        /// The metadata dictionary to validate and normalize.
        /// </param>
        /// <returns>
        /// A new immutable metadata dictionary containing only valid and normalized entries,
        /// or <see langword="null"/> when the resulting metadata is empty.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a reserved metadata key is detected.
        /// </exception>
        /// <remarks>
        /// This method:
        ///
        /// <list type="bullet">
        /// <item>
        /// <description>Trims metadata keys and string values.</description>
        /// </item>
        /// <item>
        /// <description>Removes invalid or empty entries.</description>
        /// </item>
        /// <item>
        /// <description>Converts enum values to their string representation.</description>
        /// </item>
        /// <item>
        /// <description>Preserves original object types whenever possible.</description>
        /// </item>
        /// <item>
        /// <description>Prevents reserved logging properties from being overridden through metadata.</description>
        /// </item>
        /// </list>
        ///
        /// <para>
        /// Reserved metadata keys are defined in
        /// <see cref="ReservedMetadataKeys"/> and must not be used
        /// as custom metadata entries.
        ///
        /// Reserved keys include:
        /// <c>TraceId</c>,
        /// <c>Timestamp</c>,
        /// <c>Level</c>,
        /// <c>Message</c>
        /// and <c>Source</c>.
        /// </para>
        /// 
        /// <para>
        /// Supported metadata value types include:
        /// </para>
        /// 
        /// <list type="bullet">
        /// <item><description>Primitive numeric types</description></item>
        /// <item><description><see cref="string"/></description></item>
        /// <item><description><see cref="decimal"/></description></item>
        /// <item><description><see cref="Guid"/></description></item>
        /// <item><description><see cref="DateTime"/></description></item>
        /// <item><description><see cref="DateTimeOffset"/></description></item>
        /// <item><description><see cref="TimeSpan"/></description></item>
        /// <item><description><see cref="Enum"/> values</description></item>
        /// </list>
        /// 
        /// <para>
        /// <see cref="Guid"/> values are normalized to their string representation
        /// to improve compatibility across logging providers and serialization targets.
        /// </para>
        ///
        /// <para>
        /// Complex object graphs and unsupported reference types are intentionally rejected
        /// to preserve provider compatibility, serialization consistency,
        /// and predictable metadata behavior.
        /// </para>
        ///
        /// <para>
        /// The original metadata instance is never mutated.
        /// A new immutable dictionary is created when valid entries exist.
        /// </para>
        /// </remarks>
        public static ImmutableDictionary<string, object>? EnsureValidMetadata(this ImmutableDictionary<string, object>? metadata)
        {
            if (metadata is null || metadata.Count == 0)
            {
                return null;
            }

            ImmutableDictionary<string, object>.Builder builder = ImmutableDictionary.CreateBuilder<string, object>();

            foreach ((string key, object value) in metadata)
            {
                string? normalizedKey = NormalizeKey(key);
                object? normalizedValue = NormalizeValue(value);

                if (normalizedKey is null || normalizedValue is null)
                {
                    continue;
                }

                ValidateReservedKey(normalizedKey);
                builder[normalizedKey] = normalizedValue;
            }

            return builder.Count > 0
                ? builder.ToImmutable()
                : null;
        }

        private static string? NormalizeKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
        }

        private static void ValidateReservedKey(string key)
        {
            if (ReservedMetadataKeys.Keys.Contains(key))
            {
                throw new InvalidOperationException(
                    $"Metadata key '{key}' is reserved.");
            }
        }

        private static object? NormalizeValue(object value)
        {
            return value is null
                ? null
                : !IsSupportedType(value)
                ? throw new InvalidOperationException($"Metadata type '{value.GetType().Name}' is not supported.")
                : value switch
                {
                    string text => text.Trim(),
                    Enum enumValue => enumValue.ToString(),
                    Guid guidValue => guidValue.ToString(),
                    _ => value
                };
        }

        private static bool IsSupportedType(object value)
        {
            Type type = value.GetType();

            return type.IsPrimitive
                || value is string
                || value is decimal
                || value is Guid
                || value is DateTime
                || value is DateTimeOffset
                || value is TimeSpan
                || value is Enum;
        }
    }
}
