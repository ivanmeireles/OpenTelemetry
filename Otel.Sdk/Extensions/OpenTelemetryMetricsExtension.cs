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
using System.Threading.Tasks;


namespace Otel.Sdk.Extensions
{
    public static class OpenTelemetryMetricsExtension
    {
        internal static Meter METER;
        internal static Histogram<int> HTTP_REQUEST_ELAPSED_TIME;
        internal static Counter<int> HEART_BEAT;

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

        private static void StartHeartBeat()
        {
            Task.Run(async() => {
                while (true)
                {

                    HEART_BEAT.Add(1);
                    await Task.Delay(30000);
                }
            });
        }

        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            if (METER is null)
                throw new MetricStartupException();

            HEART_BEAT = METER.CreateCounter<int>("heart_beat");
            HTTP_REQUEST_ELAPSED_TIME = METER.CreateHistogram<int>("http_request_elapsed_time");
            StartHeartBeat();

            return builder.UseMiddleware<HttpRequestMetricsMiddleware>();
        }
    }
}
