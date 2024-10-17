using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Otel.MassTransit.Publish
{
    public class LogPublishObserver : IPublishObserver
    {
        private readonly ILogger<TracePublishObserver> _logger;

        public LogPublishObserver(ILogger<TracePublishObserver> logger)
        {
            _logger = logger;
        }

        public async Task PrePublish<T>(PublishContext<T> context) where T : class
        {
        }

        public async Task PostPublish<T>(PublishContext<T> context) where T : class
        {
        }

        public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            _logger.LogError(exception.Message, exception);
        }
    }
}
