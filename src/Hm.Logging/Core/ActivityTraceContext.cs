using System.Diagnostics;
using Hm.Logging.Abstractions;

namespace Hm.Logging.Core;

/// <summary>
/// Default trace context implementation based on <see cref="Activity"/>.
/// </summary>
/// <remarks>
/// Uses <see cref="Activity.Current"/> to retrieve
/// the current distributed tracing identifier.
/// </remarks>
internal sealed class ActivityTraceContext : ITraceContext
{
    public string? GetTraceId()
        => Activity.Current?.TraceId.ToString();
}
