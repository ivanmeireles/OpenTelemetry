using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
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
            StringValues correlationIdValue = default;
            context.Request.Headers.TryGetValue(ConstValues.KEY_CORRELATION_ID, out correlationIdValue);

            var correlationId = correlationIdValue.ToString();
            if (string.IsNullOrEmpty(correlationId))
                correlationId = "none";

            var activity = Activity.Current;

            Activity.Current?.SetTag(ConstValues.KEY_CORRELATION_ID, correlationId);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }
    }
}
