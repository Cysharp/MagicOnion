using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Exporter.Prometheus;

namespace ChatApp.Server
{
    /// <summary>
    /// A HTTP listener used to expose Prometheus metrics.
    /// </summary>
    sealed class PrometheusExporterMetricsHttpServerCustom : IDisposable
    {
        private readonly PrometheusExporter exporter;
        private readonly HttpListener httpListener = new HttpListener();
        private readonly object lck = new object();

        private CancellationTokenSource tokenSource;
        private Task workerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrometheusExporterMetricsHttpServer"/> class.
        /// </summary>
        /// <param name="exporter">The <see cref="PrometheusExporter"/> instance.</param>
        /// <param name="listenerUrl">listener endpoint</param>
        public PrometheusExporterMetricsHttpServerCustom(PrometheusExporter exporter, string listenerUrl)
        {
            this.exporter = exporter;
            this.httpListener.Prefixes.Add(listenerUrl);
        }

        /// <summary>
        /// Start exporter.
        /// </summary>
        /// <param name="token">An optional <see cref="CancellationToken"/> that can be used to stop the htto server.</param>
        public void Start(CancellationToken token = default)
        {
            lock (this.lck)
            {
                if (this.tokenSource != null)
                {
                    return;
                }

                // link the passed in token if not null
                this.tokenSource = token == default ?
                    new CancellationTokenSource() :
                    CancellationTokenSource.CreateLinkedTokenSource(token);

                this.workerThread = Task.Factory.StartNew((Action)this.WorkerThread, TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Stop exporter.
        /// </summary>
        public void Stop()
        {
            lock (this.lck)
            {
                if (this.tokenSource == null)
                {
                    return;
                }

                this.tokenSource.Cancel();
                this.workerThread.Wait();
                this.tokenSource = null;
            }
        }

        /// <summary>
        /// Disposes of managed resources.
        /// </summary>
        public void Dispose()
        {
            if (this.httpListener != null && this.httpListener.IsListening)
            {
                this.Stop();
            }
        }

        private void WorkerThread()
        {
            this.httpListener.Start();

            try
            {
                while (!this.tokenSource.IsCancellationRequested)
                {
                    var ctxTask = this.httpListener.GetContextAsync();
                    ctxTask.Wait(this.tokenSource.Token);

                    var ctx = ctxTask.Result;

                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/plain; version = 0.0.4";

                    using (var output = ctx.Response.OutputStream)
                    using (var writer = new StreamWriter(output))
                    {
                        this.exporter.WriteMetricsCollection(writer);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // this will happen when cancellation will be requested
            }
            catch (Exception)
            {
                // TODO: report error
            }
            finally
            {
                this.httpListener.Stop();
                this.httpListener.Close();
            }
        }
    }
}
