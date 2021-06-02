using Grpc.Core;
using MagicOnion.Client;
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
            using var rpcScope = new ClientRpcScope(context, source);
            rpcScope.SetAdditionalTags(options.TracingTags);

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
        private readonly RequestContext context;
        private readonly ActivitySource source;

        public ClientRpcScope(RequestContext context, ActivitySource source) : base(context.MethodPath, context.MethodPath)
        {
            this.context = context;
            this.source = source;

            // capture the current activity
            this.ParentActivity = Activity.Current;

            if (!source.HasListeners())
                return;

            var rpcActivity = source.StartActivity(
                context.MethodPath,
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
