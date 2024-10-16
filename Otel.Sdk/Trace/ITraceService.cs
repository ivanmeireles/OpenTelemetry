using System.Diagnostics;

namespace Otel.Sdk.Trace
{
    public interface ITraceService
    {
        ActivitySource GetActivitySource();
        Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal, string externalTraceId = null);
    }
}