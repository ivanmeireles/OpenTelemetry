using System;

namespace Otel.Sdk.Exceptions
{
    public class MetricStartupException : Exception
    {
        public MetricStartupException()
            : base("Should be configure the AddOtlpMetrics before.") { }
    }
}
