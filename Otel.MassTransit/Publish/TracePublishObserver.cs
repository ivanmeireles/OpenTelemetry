using MassTransit;
using Otel.Sdk.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otel.MassTransit.Publish
{
    public class TracePublishObserver : IPublishObserver
    {
        private readonly ITraceService _traceService;

        public TracePublishObserver(ITraceService traceService)
        {
            _traceService = traceService;
        }

        public async Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            var activatyId = Activity.Current?.Id;
            if (string.IsNullOrEmpty(activatyId) is false)
                context.Headers.Set(Const.HEADER_TRACE_ID, activatyId);

            context.Headers.Set(Const.HEADER_PUBLISH, "true");
            context.Headers.Set(Const.HEADER_SEND, "false");
        }

        public async Task PostPublish<T>(PublishContext<T> context) where T : class
        {
        }

        public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
        }
    }
}
