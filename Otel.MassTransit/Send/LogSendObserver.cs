using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Otel.MassTransit.Send
{
    public class LogSendObserver : ISendObserver
    {
        private readonly ILogger<LogSendObserver> _logger;

        public LogSendObserver(ILogger<LogSendObserver> logger)
        {
            _logger = logger;
        }

        public async Task PreSend<T>(SendContext<T> context) where T : class
        {
        }

        public async Task PostSend<T>(SendContext<T> context) where T : class
        {
        }

        public async Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            try
            {
                _logger.LogError(exception.Message, exception);
            }
            catch
            {
            }
        }
    }
}
