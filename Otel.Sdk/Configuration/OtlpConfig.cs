﻿using System.Collections.Generic;

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


        public List<string> TraceIgnoreApplicationPathStartWith { get; set; }
        public MetricConfig MetricConfig { get; set; }

    }

    public class MetricConfig
    {
        public bool RuntimeInstrumentationDisabled { get; set; }
        public bool HttpClientInstrumentationDisabled { get; set; }
        public bool AspNetCoreInstrumentationDisabled { get; set; }
    }
}
