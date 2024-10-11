using Otel.Sdk.Exceptions;
using Otel.Sdk.Extensions;
using System.Diagnostics;

namespace Otel.Sdk.Trace
{
    public sealed class TraceService : ITraceService
    {
        public ActivitySource GetActivitySource()
        {
            if (OpenTelemetryTracingExtension.ACTIVITY_SOURCE is null)
                throw new TraceStartupException();

            return OpenTelemetryTracingExtension.ACTIVITY_SOURCE;
        }

        public Activity StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal)
        {
            if (OpenTelemetryTracingExtension.ACTIVITY_SOURCE is null)
                throw new TraceStartupException();
            
            return GetActivitySource().StartActivity(name, kind);
        }
    }
}
