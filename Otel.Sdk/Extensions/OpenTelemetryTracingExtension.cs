using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Otel.Sdk.Configuration;
using Otel.Sdk.Trace;
using System;
using System.Diagnostics;

namespace Otel.Sdk.Extensions
{
    public static class OpenTelemetryTracingExtension
    {
        internal static ActivitySource ACTIVITY_SOURCE;

        public static TracerProviderBuilder AddOtlpTracing(this IServiceCollection services, OtlpConfig config)
        {
            ACTIVITY_SOURCE = new ActivitySource(config.ServiceName, config.ServiceVersion);
            services.AddSingleton(ACTIVITY_SOURCE);
            services.AddSingleton<ITraceService, TraceService>();

            TracerProviderBuilder tracerBuilder = default;
            var protocol = config.IsGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
            var endpoint = protocol == OtlpExportProtocol.Grpc ? new Uri(config.Endpoint) : new Uri($"{config.Endpoint}/v1/traces");

            services.AddOpenTelemetry().WithTracing(builder =>
            {
                tracerBuilder = builder;
                builder
                     .AddOtlpExporter(opt =>
                     {
                         opt.Endpoint = endpoint;
                         opt.Protocol = protocol;
                         opt.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
                     })
                    .AddSource(config.ServiceName)
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion))
                    .AddHttpClientInstrumentation(options => {
                        if (config.TraceConfig?.IgnoreHttpClientDnsStartWith?.Count > 0)
                        {
                            foreach (var item in config.TraceConfig.IgnoreHttpClientDnsStartWith)
                            {
                                bool shouldIgnore = false;

                                options.FilterHttpRequestMessage = context =>
                                {
                                    shouldIgnore = context.Headers.Host.StartsWith(item);
                                    return !shouldIgnore;
                                };

                                if (shouldIgnore)
                                    break;
                            }
                        }
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        if (config.TraceConfig?.IgnoreServerPathStartWith?.Count > 0)
                        {
                            foreach (var item in config.TraceConfig.IgnoreServerPathStartWith)
                            {
                                bool shouldIgnore = false;
                                options.Filter = context =>
                                {
                                    shouldIgnore = context.Request.Path.ToString().StartsWith(item);
                                    return !shouldIgnore;
                                };

                                if (shouldIgnore)
                                    break;
                            }
                        }
                    });

                if (config.EnableConsoleExporter)
                    builder.AddConsoleExporter();
            });

            return tracerBuilder;
        }
    }
}
