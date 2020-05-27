using System.Threading;
using System.Threading.Tasks;
using MagicOnion.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics.Export;

namespace ChatApp.Server
{
    sealed class PrometheusExporterMetricsService : IHostedService
    {
        private readonly PrometheusExporterMetricsHttpServerCustom server;
        private readonly ILogger<PrometheusExporterMetricsService> logger;
        private readonly MagicOnionOpenTelemetryOptions options;
        private readonly string metricsExporterHostingEndpoint;

        public PrometheusExporterMetricsService(MetricExporter exporter, MagicOnionOpenTelemetryOptions options, IConfiguration configuration, ILogger<PrometheusExporterMetricsService> logger)
        {
            this.logger = logger;
            this.options = options;
            if (exporter is PrometheusExporter prometheusExporter)
            {
                metricsExporterHostingEndpoint = configuration.GetSection("MagicOnion:OpenTelemetry").GetValue("PrometheusMetricsHostingEndpoint", options.MetricsExporterEndpoint);
                server = new PrometheusExporterMetricsHttpServerCustom(prometheusExporter, metricsExporterHostingEndpoint);
            }
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (server != null)
            {
                logger.LogInformation($"PrometheusExporter MetricsServer is listening on: {metricsExporterHostingEndpoint}, sending to {options.MetricsExporterEndpoint}");
                server.Start();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (server != null)
            {
                logger.LogInformation($"Stopping PrometheusExporter MetricsServer.");
                server.Stop();
            }
            return Task.CompletedTask;
        }
    }
}
