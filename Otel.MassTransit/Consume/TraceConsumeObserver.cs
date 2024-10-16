using MassTransit;
using Otel.Sdk.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otel.MassTransit.Consume
{
    public class TraceConsumeObserver : IConsumeObserver
    {
        private readonly ITraceService _traceService;
        private Activity? activity;

        public TraceConsumeObserver(ITraceService traceService)
        {
            _traceService = traceService;
        }

        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            var wasCommand = string.IsNullOrEmpty(context.GetHeader(Const.HEADER_SEND)) == false;
            var traceId = string.Empty;

            if (wasCommand)
                traceId = context.GetHeader(Const.HEADER_TRACE_ID);

            activity = _traceService.StartActivity("Consumer", ActivityKind.Consumer, traceId);
        }

        public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            activity?.Dispose();
        }

        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            activity?.AddException(exception);
            activity?.Dispose();
        }
    }
}
