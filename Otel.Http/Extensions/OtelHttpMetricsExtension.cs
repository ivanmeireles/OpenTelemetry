using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Otel.Sdk.Exceptions;
using Otel.Sdk.HttpMiddleware;
using Otel.Sdk.Metric;

namespace Otel.Http.Extensions
{
    public static class OtelHttpMetricsExtension
    {
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            var metricService = builder.ApplicationServices.GetService<IMetricService>();
            var meter = metricService?.GetCurrentMeter();

            if (meter is null)
                throw new MetricStartupException();

            HttpRequestMetricsMiddleware.HTTP_REQUEST_ELAPSED_TIME = meter.CreateHistogram<int>("http_request_elapsed_time");
            return builder.UseMiddleware<HttpRequestMetricsMiddleware>();
        }
    }
}
