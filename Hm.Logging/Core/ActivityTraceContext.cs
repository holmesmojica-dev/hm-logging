using System.Diagnostics;
using Hm.Logging.Abstractions;

namespace Hm.Logging.Core
{
    internal sealed class ActivityTraceContext : ITraceContext
    {
        public string? GetTraceId()
        {
            return Activity.Current?.TraceId.ToString();
        }
    }
}
