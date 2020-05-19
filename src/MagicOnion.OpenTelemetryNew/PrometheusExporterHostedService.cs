using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.Prometheus;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Hosting
{
    sealed class PrometheusExporterHostedService : IHostedService
    {
        private readonly PrometheusExporter exporter;
        private PrometheusExporterMetricsHttpServer server;

        public PrometheusExporterHostedService(PrometheusExporter exporter)
        {
            this.exporter = exporter ?? throw new System.ArgumentNullException(nameof(exporter));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            server = new PrometheusExporterMetricsHttpServer(exporter);
            server.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            server.Stop();
            return Task.CompletedTask;
        }
    }

}
