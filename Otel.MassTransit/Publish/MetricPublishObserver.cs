using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Publish
{
    public class MetricPublishObserver : IPublishObserver
    {
        private Stopwatch _stopwatch;
        private readonly IMetricService _metricService;
        private IDictionary<string, object?> _tags;

        private const string TAG_SUCCESS = "success";
        private const string TAG_EXCEPTION_NAME = "exception_name";

        public MetricPublishObserver(IMetricService metricService)
        {
            _metricService = metricService;
            _tags = new Dictionary<string, object?>();
        }

        public async Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public async Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            _tags.TryAdd(TAG_SUCCESS, "true");
            DoneConsume();
        }

        public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            try
            {
                _tags.TryAdd(TAG_SUCCESS, "false");
                _tags.TryAdd(TAG_EXCEPTION_NAME, exception.GetType().Name);
                DoneConsume();
            }
            catch
            {
            }
        }

        private void DoneConsume()
        {
            _stopwatch.Stop();
            var elapsedTime = (int)_stopwatch.ElapsedMilliseconds;

            _metricService.HistogramRecord(Const.METRIC_PUBLISH_NAME, elapsedTime, _tags.ToArray());
        }
    }
}
