using MassTransit.Logging;
using OpenTelemetry.Trace;

namespace Otel.MassTransit.Extensions
{
    public static class MassTransitLoadTrancingExtension
    {
        public static TracerProviderBuilder ConfigureMassTransitTracing(this TracerProviderBuilder builder)
        {
            builder.AddSource(DiagnosticHeaders.DefaultListenerName);
            return builder;
        }
    }
}
