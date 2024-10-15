using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using Otel.Sdk.Configuration;
using Otel.Sdk.Exceptions;
using System;
using System.Diagnostics.Metrics;
using OpenTelemetry.Resources;
using Otel.Sdk.HttpMiddleware;
using Otel.Sdk.Metric;


namespace Otel.Sdk.Extensions
{
    public static class OpenTelemetryMetricsExtension
    {
        internal static Meter METER;
        internal static Counter<int> HTTP_REQUEST_COUNTER;
        internal static Counter<int> HTTP_REQUEST_200_COUNTER;
        internal static Counter<int> HTTP_REQUEST_400_COUNTER;
        internal static Counter<int> HTTP_REQUEST_500_COUNTER;
        internal static Histogram<int> HTTP_REQUEST_ELAPSED_TIME;

        public static MeterProviderBuilder AddOtlpMetrics(this IServiceCollection services, OtlpConfig config)
        {
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

        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            if (METER is null)
                throw new MetricStartupException();

            HTTP_REQUEST_COUNTER = METER.CreateCounter<int>("http_request_total_count");
            HTTP_REQUEST_200_COUNTER = METER.CreateCounter<int>("http_request_family_200_total_count");
            HTTP_REQUEST_400_COUNTER = METER.CreateCounter<int>("http_request_family_400_total_count");
            HTTP_REQUEST_500_COUNTER = METER.CreateCounter<int>("http_request_family_500_total_count");
            HTTP_REQUEST_ELAPSED_TIME = METER.CreateHistogram<int>("http_request_elaspsed_time");

            return builder.UseMiddleware<HttpRequestMetricsMiddleware>();
        }
    }
}
