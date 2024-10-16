using MassTransit;
using Otel.Sdk.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otel.MassTransit
{
    public class TraceConsumeObserver : IConsumeObserver
    {
        private readonly ITraceService _traceService;

        public TraceConsumeObserver(ITraceService traceService) 
        {
            _traceService = traceService;
        }


        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            Activity.Current?.AddException(exception);
        }

        public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
        }

        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            
        }
    }
}
