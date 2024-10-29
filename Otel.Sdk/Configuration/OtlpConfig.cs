using System.Collections.Generic;

namespace Otel.Sdk.Configuration
{
    public class OtlpConfig
    {
        public OtlpConfig()
        {
            MetricConfig = new MetricConfig();
        }

        public string ServiceName { get; set; }
        public string ServiceVersion { get; set; }
        public string Endpoint { get; set; }
        public bool IsGrpc { get; set; }
        public bool EnableConsoleExporter { get; set; }


        public TraceConfig TraceConfig { get; set; }
        public MetricConfig MetricConfig { get; set; }
    }

    public class TraceConfig
    {
        public List<string> IgnoreServerPathStartWith { get; set; }
        public List<string> IgnoreHttpClientHostStartWith { get; set; }
    }

    public class MetricConfig
    {
        public bool RuntimeInstrumentationDisabled { get; set; }
        public bool HttpClientInstrumentationDisabled { get; set; }
        public bool AspNetCoreInstrumentationDisabled { get; set; }

        public List<string> IgnorePathStartWith { get; set; }
    }
}
