using MassTransit;
using Otel.Sdk.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otel.MassTransit.Send
{
    public class TraceSendObserver : ISendObserver
    {
        private readonly ITraceService _traceService;

        public TraceSendObserver(ITraceService traceService)
        {
            _traceService = traceService;
        }

        public async Task PreSend<T>(SendContext<T> context) where T : class
        {
            var activatyId = Activity.Current?.Id;
            if (string.IsNullOrEmpty(activatyId) is false)
                context.Headers.Set(Const.HEADER_TRACE_ID, activatyId);


            context.Headers.Set(Const.HEADER_SEND, "true");
            context.Headers.Set(Const.HEADER_PUBLISH, "false");
        }

        public async Task PostSend<T>(SendContext<T> context) where T : class
        {
        }

        public async Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
        }
    }
}
