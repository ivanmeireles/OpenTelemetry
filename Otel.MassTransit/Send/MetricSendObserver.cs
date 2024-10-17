using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Send
{
    public class MetricPublishObserver : ISendObserver
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

        public async Task PreSend<T>(SendContext<T> context) where T : class
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public async Task PostSend<T>(SendContext<T> context) where T : class
        {
            _tags.TryAdd(TAG_SUCCESS, true);
            DoneConsume();
        }

        public async Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            _tags.TryAdd(TAG_SUCCESS, false);
            _tags.TryAdd(TAG_EXCEPTION_NAME, exception.GetType().Name);
            DoneConsume();
        }

        private void DoneConsume()
        {
            _stopwatch.Stop();
            var elapsedTime = (int)_stopwatch.ElapsedMilliseconds;

            _metricService.HistogramRecord(Const.METRIC_SEND_NAME, elapsedTime, _tags.ToArray());
        }
    }
}
