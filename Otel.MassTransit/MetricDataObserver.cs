using System.Collections.Generic;
using System.Diagnostics;

namespace Otel.MassTransit
{
    public class MetricDataObserver
    {
        private const string TAG_SUCCESS = "success";
        private const string TAG_EXCEPTION_NAME = "exception_name";

        private Stopwatch _stopwatch;
        private IDictionary<string, object?> _tags;
        private int elapsedMilliseconds = 0;

        public MetricDataObserver() => _tags = new Dictionary<string, object?>();
        public void StartWatch() => _stopwatch = Stopwatch.StartNew();
        public void AddTag(string name, string value) => _tags.Add(name, value);
        public int StopWatchConsumerSuccess() => StopWatch(true);
        public int StopWatchConsumerFail(string exceptionName) => StopWatch(false, exceptionName);
        public int GetElapsedMilliseconds => elapsedMilliseconds;
        public IDictionary<string, object?> Tags => _tags;


        private int StopWatch(bool hasSuccess, string exceptionName = null)
        {
            if (elapsedMilliseconds == 0)
            {

                if (hasSuccess)
                    _tags.TryAdd(TAG_SUCCESS, "true");
                else
                {
                    _tags.TryAdd(TAG_SUCCESS, "false");
                    _tags.TryAdd(TAG_EXCEPTION_NAME, exceptionName);
                }

                _stopwatch.Stop();
                elapsedMilliseconds = (int)_stopwatch.ElapsedMilliseconds;
            }

            return elapsedMilliseconds;
        }
    }
}
