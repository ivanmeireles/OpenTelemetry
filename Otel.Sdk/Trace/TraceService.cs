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

        public Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal, string? externalTraceId = null)
        {

            if (string.IsNullOrEmpty(externalTraceId))
            {
                return OpenTelemetryTracingExtension.ACTIVITY_SOURCE.StartActivity(name, kind);
            }
            
            var (parentTraceId, parentSpanId) = GetParents(externalTraceId);
            if (parentTraceId is null || parentSpanId is null)
                return StartActivity(name, kind);

            var parentContext = new ActivityContext(
                ActivityTraceId.CreateFromString(parentTraceId),
                ActivitySpanId.CreateFromString(parentSpanId),
                ActivityTraceFlags.Recorded);

            return OpenTelemetryTracingExtension.ACTIVITY_SOURCE.StartActivity(name, kind, parentContext);
        }

        private (string? parentTraceId, string? parentSpanId) GetParents(string externalTraceId)
        {
            try
            {
                var traceValues = externalTraceId.Split('-');
                if (traceValues.Length > 0)
                {
                    return (traceValues[1], traceValues[2]);
                }
            }
            catch
            {
                // ignored
            }
            return (null, null);
        }
    }
}
