using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Send
{
    public class MetricSendObserver : ISendObserver
    {
        private readonly IMetricService _metricService;

        public MetricSendObserver(IMetricService metricService)
            => _metricService = metricService;

        public Task PreSend<T>(SendContext<T> context) where T : class
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

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            if (context.TryGetPayload(out MetricDataObserver metricData))
            {
                metricData.StopWatchConsumerSuccess();
                DoneConsume(metricData);
            }
            return Task.CompletedTask;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
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
                Const.METRIC_SEND_NAME,
                metricData.GetElapsedMilliseconds,
                metricData.Tags.ToArray()
            );
        }
    }
}
