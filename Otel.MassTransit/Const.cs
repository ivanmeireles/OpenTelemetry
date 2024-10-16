namespace Otel.MassTransit
{
    public static class Const
    {
        public const string HEADER_TRACE_ID = "x_otel_trace_id";
        public const string HEADER_SEND = "x_otel_send";
        public const string HEADER_PUBLISH = "x_otel_publish";

        public const string METRIC_CONSUMER_NAME = "masstransit_consumer";
    }
}
