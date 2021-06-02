using Grpc.Core;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MagicOnion.Server.OpenTelemetry
{
    internal abstract class RpcScope : IDisposable
    {
        private Activity activity;
        private long complete = 0;

        protected string RpcService { get; }
        protected string RpcMethod { get; }
        protected Activity ParentActivity { get; set; }

        protected RpcScope(string rpcService, string rpcMethod)
        {
            this.RpcService = rpcService;
            this.RpcMethod = rpcMethod;
        }

        /// <summary>
        /// Call <see cref="Complete"/> or <see cref="CompleteWithException(Exception)"/> to set activity status.
        /// Without complete will records a cancel RPC
        /// </summary>
        public void Dispose()
        {
            if (activity == null)
            {
                return;
            }

            // If not already completed this will mark the Activity as cancelled.
            StopActivity((int)Grpc.Core.StatusCode.Cancelled);
        }

        /// <summary>
        /// Records a complete RPC
        /// </summary>
        public void Complete()
        {
            if (activity == null)
            {
                return;
            }

            // The overall Span status should remain unset however the grpc status code attribute is required
            StopActivity((int)Grpc.Core.StatusCode.OK);
        }

        /// <summary>
        /// Records a failed RPC
        /// </summary>
        /// <param name="exception"></param>
        public void CompleteWithException(Exception exception)
        {
            if (this.activity == null)
            {
                return;
            }

            var grpcStatusCode = Grpc.Core.StatusCode.Unknown;
            var description = exception.Message;

            if (exception is RpcException rpcException)
            {
                grpcStatusCode = rpcException.StatusCode;
                description = rpcException.Message;
            }

            this.StopActivity((int)grpcStatusCode, description);
        }
        protected void SetActivity(Activity activity)
        {
            this.activity = activity;

            if (!this.activity.IsAllDataRequested)
            {
                return;
            }

            this.activity.SetTag(SemanticConventions.AttributeRpcSystem, "grpc");
            this.activity.SetTag(SemanticConventions.AttributeRpcService, RpcService);
            this.activity.SetTag(SemanticConventions.AttributeRpcMethod, RpcMethod);
        }

        public void SetAdditionalTags(IDictionary<string, string> tags)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }

        private void StopActivity(int statusCode, string statusDescription = null)
        {
            if (Interlocked.CompareExchange(ref this.complete, 1, 0) == 0)
            {
                this.activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, statusCode);
                if (statusDescription != null)
                {
                    this.activity.SetStatus(global::OpenTelemetry.Trace.Status.Error.WithDescription(statusDescription));
                }

                this.activity.Stop();
            }
        }
    }
}
