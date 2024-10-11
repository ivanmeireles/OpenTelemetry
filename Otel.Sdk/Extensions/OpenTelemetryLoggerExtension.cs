using Microsoft.Extensions.Logging;
using Otel.Sdk.Configuration;
using System;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Otel.Sdk.Extensions
{
    public static class OpenTelemetryLoggerExtension
    {
        public static ILoggingBuilder AddOtlpLogging(this ILoggingBuilder builder, OtlpConfig config)
        {
            var protocol = config.IsGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
            var endpoint = protocol == OtlpExportProtocol.Grpc ? new Uri(config.Endpoint) : new Uri($"{config.Endpoint}/v1/logs");

            builder.AddOpenTelemetry((OpenTelemetryLoggerOptions loggingbuilder) =>
            {
                loggingbuilder.AddOtlpExporter("otlp_logging", (OtlpExporterOptions opt) =>
                {
                    opt.Endpoint = endpoint;
                    opt.Protocol = protocol;
                    opt.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;

                }).SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion));

                if (config.EnableConsoleExporter)
                    loggingbuilder.AddConsoleExporter();
            });

            return builder;
        }
    }
}
