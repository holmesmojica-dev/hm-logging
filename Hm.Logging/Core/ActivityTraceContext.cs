using System.Diagnostics;
using Hm.Logging.Abstractions;

namespace Hm.Logging.Core
{
    /// <summary>
    /// Default trace context implementation based on <see cref="Activity"/>.
    /// </summary>
    internal sealed class ActivityTraceContext : ITraceContext
    {
        public string? GetTraceId()
        {
            return Activity.Current?.TraceId.ToString();
        }
    }
}
