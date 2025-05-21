using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Otel.Sdk.Extensions;
using Otel.Sdk.Metric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otel.Sdk.HttpMiddleware
{
    internal class HttpRequestMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpRequestMetricsMiddleware> _logger;

        public HttpRequestMetricsMiddleware(RequestDelegate next, ILogger<HttpRequestMetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value.ToLower();
            if (OpenTelemetryMetricsExtension.OTLP_CONFIG?.MetricConfig?.IgnorePathStartWith?.Count > 0)
            {
                var shouldIgnoreMetrics = false;
                foreach (var ignorePathStartWith in OpenTelemetryMetricsExtension.OTLP_CONFIG.MetricConfig.IgnorePathStartWith)
                {
                    shouldIgnoreMetrics = path.StartsWith(ignorePathStartWith);
                    if (shouldIgnoreMetrics)
                        break;
                }

                if (shouldIgnoreMetrics)
                {
                    await _next(context);
                    return;
                }
            }

            var activity = Activity.Current;
            Stopwatch watch = Stopwatch.StartNew();
            List<KeyValuePair<string, object?>> listTags = new List<KeyValuePair<string, object>>();

            listTags.Add(MetricService.CreateTag("http_method", context.Request.Method));

            if (activity != null)
            {
                activity.SetTag(ConstValues.KEY_CORRELATION_ID, GetCorrelationId(context));
                string value = context.Connection.RemoteIpAddress.ToString();
                activity.SetTag("http_remote_ip_address", value);
            }

            try
            {
                await _next(context);
                int statusCode = context.Response.StatusCode;
                listTags.Add(MetricService.CreateTag("http_status_code", statusCode));
                if (statusCode >= 200 && statusCode <= 299)
                {
                    listTags.Add(MetricService.CreateTag("http_status_code_family", "200"));
                }
                else if (statusCode >= 400 && statusCode <= 499)
                {
                    listTags.Add(MetricService.CreateTag("http_status_code_family", "400"));
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = ex.InnerException ?? ex;
                listTags.Add(MetricService.CreateTag("http_status_code_family", "500"));
                listTags.Add(MetricService.CreateTag("exception_full_name", ex2.GetType().FullName));
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
            finally
            {
                watch.Stop();
                int value2 = (int)watch.ElapsedMilliseconds;
                OpenTelemetryMetricsExtension.HTTP_REQUEST_ELAPSED_TIME.Record(value2, listTags.ToArray());
            }
        }

        private string GetCorrelationId(HttpContext context)
        {
            StringValues correlationIdValue = default;
            context.Request.Headers.TryGetValue(ConstValues.KEY_CORRELATION_ID, out correlationIdValue);

            var correlationId = correlationIdValue.ToString();
            if (string.IsNullOrEmpty(correlationId))
                correlationId = "none";

            return correlationId;
        }
    }
}
