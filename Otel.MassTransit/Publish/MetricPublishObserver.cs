using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Publish
{
    public class MetricPublishObserver : IPublishObserver
    {
        private readonly IMetricService _metricService;

        public MetricPublishObserver(IMetricService metricService)
            => _metricService = metricService;

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            context.GetOrAddPayload(() =>
            {
                var metricData = new MetricDataObserver();
                metricData.StartWatch();

                var queueName = context.DestinationAddress?.AbsolutePath;
                if (queueName != null)
                {
                    metricData.AddTag(Const.TAG_QUEUE_NAME, queueName);
                }

                return metricData;
            });
            return Task.CompletedTask;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            if (context.TryGetPayload(out MetricDataObserver metricData))
            {
                metricData.StopWatchConsumerSuccess();
                DoneConsume(metricData);
            }
            return Task.CompletedTask;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            try
            {
                if (context.TryGetPayload(out MetricDataObserver metricData))
                {
                    metricData.StopWatchConsumerFail(exception.GetType().Name);
                    DoneConsume(metricData);
                }
            }
            catch
            {
            }
            return Task.CompletedTask;
        }

        private void DoneConsume(MetricDataObserver metricData)
        {
            _metricService.HistogramRecord(
                Const.METRIC_PUBLISH_NAME,
                metricData.GetElapsedMilliseconds,
                metricData.Tags.ToArray()
            );
        }
    }
}
