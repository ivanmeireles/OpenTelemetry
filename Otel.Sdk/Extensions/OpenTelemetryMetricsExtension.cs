using System;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Otel.Sdk.Configuration;
using Otel.Sdk.Metric;


namespace Otel.Sdk.Extensions
{
    public static class OpenTelemetryMetricsExtension
    {
        internal static Meter METER;
        public static OtlpConfig OTLP_CONFIG;

        public static MeterProviderBuilder AddOtlpMetrics(this IServiceCollection services, OtlpConfig config)
        {
            OTLP_CONFIG = config;

            MeterProviderBuilder meterBuilder = default;
            METER = new Meter(config.ServiceName, config.ServiceVersion);

            services.AddSingleton(METER);
            services.AddSingleton<IMetricService, MetricService>();

            var protocol = config.IsGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
            var endpoint = protocol == OtlpExportProtocol.Grpc ? new Uri(config.Endpoint) : new Uri($"{config.Endpoint}/v1/metrics");

            services.AddOpenTelemetry().WithMetrics(builder =>
            {
                meterBuilder = builder;
                builder.AddMeter(config.ServiceName, config.ServiceVersion);
                builder.SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion));

                builder.AddOtlpExporter((OtlpExporterOptions opt, MetricReaderOptions metricOpt) =>
                {
                    opt.Endpoint = endpoint;
                    opt.Protocol = protocol;
                    opt.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;

                    metricOpt.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions()
                    {
                        ExportIntervalMilliseconds = 10000
                    };
                });
             
                if (config.MetricConfig.HttpClientInstrumentationDisabled == false)
                    builder.AddHttpClientInstrumentation();

                if (config.MetricConfig.RuntimeInstrumentationDisabled == false)
                    builder.AddRuntimeInstrumentation();

                if (config.MetricConfig.AspNetCoreInstrumentationDisabled == false)
                    builder.AddAspNetCoreInstrumentation();


                if (config.EnableConsoleExporter)
                    builder.AddConsoleExporter();
            });

            
            return meterBuilder;
        }
    }
}
