using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server.OpenTelemetry.Internal;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MagicOnion.Server.OpenTelemetry
{
    // note: move package to MagicOnion.Client.OpenTelemetry?
    /// <summary>
    /// Collect OpenTelemetry Tracer with Client filter (Unary).
    /// </summary>
    public class MagicOnionOpenTelemetryClientFilter : IClientFilter
    {
        readonly ActivitySource source;
        readonly MagicOnionOpenTelemetryOptions options;

        public MagicOnionOpenTelemetryClientFilter(ActivitySource activitySource, MagicOnionOpenTelemetryOptions options)
        {
            this.source = activitySource;
            this.options = options;
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            var rpcService = context.MethodPath.Split('/')[0];
            using var rpcScope = new ClientRpcScope(rpcService, context.MethodPath, context, source, options);
            rpcScope.SetTags(options.TracingTags);

            try
            {
                var response = await next(context);

                rpcScope.Complete();
                return response;
            }
            catch (Exception ex)
            {
                rpcScope.CompleteWithException(ex);
                throw;
            }
            finally
            {
                rpcScope.RestoreParentActivity();
            }
        }
    }

    internal class ClientRpcScope : RpcScope
    {
        public ClientRpcScope(string rpcService, string rpcMethod, RequestContext context, ActivitySource source, MagicOnionOpenTelemetryOptions options)
            : base(rpcService, rpcMethod, options.ServiceName)
        {
            // capture the current activity
            this.ParentActivity = Activity.Current;

            if (!source.HasListeners())
                return;

            var rpcActivity = source.StartActivity(
                context.MethodPath.TrimStart('/'),
                ActivityKind.Client,
                ParentActivity == default ? default : ParentActivity.Context);

            if (rpcActivity == null)
                return;

            var callOptions = context.CallOptions;
            if (callOptions.Headers == null)
            {
                callOptions = callOptions.WithHeaders(new Metadata());
            }

            SetActivity(rpcActivity);

            Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(rpcActivity.Context, Baggage.Current), callOptions);
        }

        /// <summary>
        /// Restores the parent activity.
        /// </summary>
        public void RestoreParentActivity()
        {
            Activity.Current = this.ParentActivity;
        }
    }
}
