using Otel.Sdk.Extensions;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Otel.Sdk.Metric
{
    public class MetricService : IMetricService
    {
        private static readonly IDictionary<string, Instrument> metrics = new Dictionary<string, Instrument>();

        public Meter GetCurrentMeter() => OpenTelemetryMetricsExtension.METER;

        public KeyValuePair<string, object?> CreateCustomTag(string name, object? value)
            => CreateTag(name, value);

        public void CounterAdd<T>(string meterName, T value, params KeyValuePair<string, object?>[] tags) where T : struct
        {
            Counter<T>? instrument;
            if (metrics.ContainsKey(meterName))
                instrument = metrics[meterName] as Counter<T>;
            else
            {
                instrument = GetCurrentMeter().CreateCounter<T>(meterName);
                metrics.Add(meterName, instrument);
            }

            instrument?.Add(value);
        }

        public void HistogramRecord<T>(string meterName, T value, params KeyValuePair<string, object?>[] tags) where T : struct
        {
            Histogram<T>? instrument;
            if (metrics.ContainsKey(meterName))
                instrument = metrics[meterName] as Histogram<T>;
            else
            {
                instrument = GetCurrentMeter().CreateHistogram<T>(meterName);
                metrics.Add(meterName, instrument);
            }

            instrument?.Record(value, tags);
        }

        public static KeyValuePair<string, object?> CreateTag(string name, object? value)
            => new KeyValuePair<string, object?>(name, value);
    }
}
