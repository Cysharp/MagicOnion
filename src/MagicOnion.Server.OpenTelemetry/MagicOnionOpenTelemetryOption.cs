using System;
using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Export;

namespace MagicOnion.Server.OpenTelemetry
{
    public class MagicOnionOpenTelemetryOptions
    {
        /// <summary>
        /// Metrics Exporter Endpoint. Default Prometheus endpoint.
        /// </summary>
        public string MetricsExporterEndpoint { get; set; } = "http://127.0.0.1:9184/metrics/";
        /// <summary>
        /// Metrics Exporter Hosting Endpoint.
        /// </summary>
        public string MetricsExporterHostingEndpoint { get; set; } = "http://+:9184/metrics/";
        /// <summary>
        /// Tracer ServiceName use as ActivitySource
        /// </summary>
        public string MagicOnionActivityName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
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
        /// <summary>
        /// MagicOnionLogger to collect OpenTelemetry metrics.
        /// </summary>
        public Func<MeterProvider, IMagicOnionLogger> MeterLogger { get; set; }
    }
}