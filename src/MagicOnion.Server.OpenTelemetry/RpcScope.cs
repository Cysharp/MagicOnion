using Grpc.Core;
using MagicOnion.Server.OpenTelemetry.Internal;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MagicOnion.Server.OpenTelemetry
{
    /// <summary>
    /// Manage trace activity scope for this Rpc
    /// </summary>
    public interface IRpcScope
    {
        /// <summary>
        /// Set custom tag to the activity.
        /// </summary>
        /// <param name="tags"></param>
        void SetTags(IDictionary<string, string> tags);
    }
    internal abstract class RpcScope : IDisposable, IRpcScope
    {
        private Activity activity;
        private long complete = 0;

        protected string RpcService { get; }
        protected string RpcMethod { get; }
        protected string ServiceName { get; }
        protected Activity ParentActivity { get; set; }

        protected RpcScope(string rpcService, string rpcMethod, string serviceName)
        {
            RpcService = rpcService;
            RpcMethod = rpcMethod;
            ServiceName = serviceName;
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
        public void Complete(Grpc.Core.StatusCode statusCode = Grpc.Core.StatusCode.OK)
        {
            if (activity == null)
            {
                return;
            }

            // The overall Span status should remain unset however the grpc status code attribute is required
            StopActivity((int)statusCode);
        }

        /// <summary>
        /// Records a failed RPC
        /// </summary>
        /// <param name="exception"></param>
        public void CompleteWithException(Exception exception)
        {
            if (activity == null)
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

            activity.SetTag(SemanticConventions.AttributeException, exception.ToString());
            StopActivity((int)grpcStatusCode, description);
        }
        protected void SetActivity(Activity activity)
        {
            this.activity = activity;

            if (!this.activity.IsAllDataRequested)
            {
                return;
            }

            this.activity.SetTag(SemanticConventions.AttributeServiceName, ServiceName);
            this.activity.SetTag(SemanticConventions.AttributeRpcSystem, "grpc");
            this.activity.SetTag(SemanticConventions.AttributeRpcService, RpcService);
            this.activity.SetTag(SemanticConventions.AttributeRpcMethod, RpcMethod);
        }

        public void SetTags(IDictionary<string, string> tags)
        {
            if (activity == null)
            {
                return;
            }

            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }

        private void StopActivity(int statusCode, string statusDescription = null)
        {
            if (Interlocked.CompareExchange(ref this.complete, 1, 0) == 0)
            {
                activity.SetTag(SemanticConventions.AttributeRpcGrpcStatusCode, statusCode);
                if (statusDescription != null)
                {
                    activity.SetStatus(global::OpenTelemetry.Trace.Status.Error.WithDescription(statusDescription));
                }

                activity.Stop();
            }
        }
    }
}
