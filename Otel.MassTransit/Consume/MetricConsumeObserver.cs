using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Consume
{
    public class MetricConsumeObserver : IConsumeObserver
    {
        private readonly IMetricService _metricService;

        public MetricConsumeObserver(IMetricService metricService)
            => _metricService = metricService;

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            context.GetOrAddPayload(() =>
            {
                var metricData = new MetricDataObserver();
                metricData.StartWatch();

                var queueName = context.ReceiveContext.InputAddress?.AbsolutePath;
                if (queueName != null)
                {
                    metricData.AddTag(Const.TAG_QUEUE_NAME, queueName);
                }

                return metricData;
            });
            return Task.CompletedTask;
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            if (context.TryGetPayload(out MetricDataObserver metricData))
            {
                metricData.StopWatchConsumerSuccess();
                DoneConsume(metricData);
            }
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
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
                Const.METRIC_CONSUMER_NAME,
                metricData.GetElapsedMilliseconds,
                metricData.Tags.ToArray()
            );
        }
    }
}
