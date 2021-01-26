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
    internal class PrometheusExporterMetricsHttpServerCustom : IDisposable
    {
        private readonly PrometheusExporter exporter;
        private readonly HttpListener httpListener = new HttpListener();
        private readonly object syncObject = new object();

        private CancellationTokenSource tokenSource;
        private Task workerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrometheusExporterMetricsHttpServer"/> class.
        /// </summary>
        /// <param name="exporter">The <see cref="PrometheusExporter"/> instance.</param>
        public PrometheusExporterMetricsHttpServerCustom(PrometheusExporter exporter, string listenerUrl)
        {
            this.exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
            this.httpListener.Prefixes.Add(listenerUrl);
        }

        /// <summary>
        /// Start exporter.
        /// </summary>
        /// <param name="token">An optional <see cref="CancellationToken"/> that can be used to stop the htto server.</param>
        public void Start(CancellationToken token = default)
        {
            lock (this.syncObject)
            {
                if (this.tokenSource != null)
                {
                    return;
                }

                // link the passed in token if not null
                this.tokenSource = token == default ?
                    new CancellationTokenSource() :
                    CancellationTokenSource.CreateLinkedTokenSource(token);

                this.workerThread = Task.Factory.StartNew(this.WorkerThread, default, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Stop exporter.
        /// </summary>
        public void Stop()
        {
            lock (this.syncObject)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.httpListener != null && this.httpListener.IsListening)
            {
                this.Stop();
                this.httpListener.Close();
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

                    using var output = ctx.Response.OutputStream;
                    using var writer = new StreamWriter(output);
                    this.exporter.WriteMetricsCollection(writer);
                }
            }
            catch (OperationCanceledException ex)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.httpListener.Stop();
                this.httpListener.Close();
            }
        }
    }
}
