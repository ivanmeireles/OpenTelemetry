using System;

namespace Otel.Sdk.Exceptions
{
    internal class TraceStartupException : Exception
    {
        public TraceStartupException()
            : base("Should be configure the AddOtlpTracing before.") { }
    }
}
