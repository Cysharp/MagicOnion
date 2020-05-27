using System;
using System.Reflection;
using OpenTelemetry.Metrics.Export;

namespace MagicOnion.OpenTelemetry
{
    public class MagicOnionOpenTelemetryOptions
    {
        /// <summary>
        /// Service Name for the app. default is Assembly name.
        /// </summary>
        public string ServiceName { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
        /// <summary>
        /// Metrics Exporter Endpoint. Default Prometheus endpoint.
        /// </summary>
        public string MetricsExporterEndpoint { get; set; } = "http://127.0.0.1:9181/metrics/";
        /// <summary>
        /// Tracer Exporter Endpoint. Default Zipkin endpoint.
        /// </summary>
        public string TracerExporterEndpoint { get; set; } = "http://127.0.0.1:9411/api/v2/spans";
    }

    public class MagicOnionOpenTelemetryMeterFactoryOption
    {
        /// <summary>
        /// OpenTelemetry MetricsProcessor. default is <see cref="UngroupedBatcher"/>
        /// </summary>
        public MetricProcessor MetricProcessor { get; set; } = new UngroupedBatcher();
        /// <summary>
        /// OpenTelemetry MetricsExporter Implementation to use.
        /// </summary>
        public MetricExporter MetricExporter { get; set; }
        /// <summary>
        /// OpenTelemetry Metric Push Interval.
        /// </summary>
        public TimeSpan MetricPushInterval { get; set; } = TimeSpan.FromSeconds(10);
    }
}