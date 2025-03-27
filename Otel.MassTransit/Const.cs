namespace Otel.MassTransit
{
    public static class Const
    {
        public const string HEADER_TRACE_ID = "x_otel_trace_id";
        public const string HEADER_SEND = "x_otel_send";
        public const string HEADER_PUBLISH = "x_otel_publish";

        public const string METRIC_CONSUMER_NAME = "masstransit_consumer";
        public const string METRIC_SEND_NAME = "masstransit_send";
        public const string METRIC_PUBLISH_NAME = "masstransit_publish";

        public const string TAG_QUEUE_NAME = "queue_name";
    }
}
