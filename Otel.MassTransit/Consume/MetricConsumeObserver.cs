using MassTransit;
using Otel.Sdk.Metric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Otel.MassTransit.Consume
{
    public class MetricConsumeObserver : IConsumeObserver
    {
        private readonly IMetricService _metricService;
        private Activity? _activity;
        private Stopwatch _stopwatch;
        private IDictionary<string, object?> _tags;

        private const string TAG_SUCCESS = "success";
        private const string TAG_EXCEPTION_NAME = "exception_name";

        public MetricConsumeObserver(IMetricService metricService)
        {
            _metricService = metricService;
            _tags = new Dictionary<string, object?>();
        }

        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            _tags.TryAdd(TAG_SUCCESS, true);
            DoneConsume();
        }

        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            _tags.TryAdd(TAG_SUCCESS, false);
            DoneConsume();
        }

        private void DoneConsume()
        {
            _stopwatch.Stop();
            var elapsedTime = (int)_stopwatch.ElapsedMilliseconds;

            _metricService.HistogramRecord(Const.METRIC_CONSUMER_NAME, elapsedTime, _tags.ToArray());
        }
    }
}
