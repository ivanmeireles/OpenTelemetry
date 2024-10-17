using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Otel.MassTransit.Consume
{
    public class LogConsumeObserver : IConsumeObserver
    {
        private readonly ILogger<LogConsumeObserver> _logger;

        public LogConsumeObserver(ILogger<LogConsumeObserver> logger)
        {
            _logger = logger;
        }

        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
        }

        public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
        }

        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            _logger.LogError(exception.Message, exception);
        }
    }
}
