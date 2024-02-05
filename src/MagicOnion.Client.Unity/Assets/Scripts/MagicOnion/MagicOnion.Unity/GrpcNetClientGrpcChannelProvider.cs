#if !MAGICONION_USE_GRPC_CCORE
using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using MagicOnion.Unity;

namespace MagicOnion.Client
{
    /// <summary>
    /// Provide and manage gRPC channels using Grpc.Net.Client for MagicOnion.
    /// </summary>
    public class GrpcNetClientGrpcChannelProvider : GrpcChannelProviderBase
    {
        readonly Func<GrpcChannelOptions> defaultChannelOptionsFactory;
        public GrpcNetClientGrpcChannelProvider()
            : this(() => new GrpcChannelOptions())
        {
        }

        [Obsolete("Use constructor with a GrpcChannelOptions factory overload instead. If you pass a GrpcChannelOptions directly, HttpClient/HttpHandler may be reused unintentionally.")]
        public GrpcNetClientGrpcChannelProvider(GrpcChannelOptions options)
            : this(() => options)
        {
        }

        public GrpcNetClientGrpcChannelProvider(Func<GrpcChannelOptions> optionsFactory)
        {
            defaultChannelOptionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        }

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        protected override GrpcChannelx CreateChannelCore(int id, CreateGrpcChannelContext context)
        {
            var address = new Uri((context.Target.IsInsecure ? "http" : "https") + $"://{context.Target.Host}:{context.Target.Port}");
            var channelOptions = context.ChannelOptions.GetOrDefault<GrpcChannelOptions>() ?? defaultChannelOptionsFactory();
            var channel = GrpcChannel.ForAddress(address, channelOptions);
            var channelHolder = new GrpcChannelx(
                id,
                context.Provider.UnregisterChannel /* Root provider may be wrapped outside this provider class. */,
                channel,
                address,
                new GrpcChannelOptionsBag(new GrpcChannelOptionsValueProvider(channelOptions))
            );

            return channelHolder;
        }

        class GrpcChannelOptionsValueProvider : IChannelOptionsValueProvider
        {
            readonly GrpcChannelOptions options;

            public GrpcChannelOptionsValueProvider(GrpcChannelOptions options)
            {
                this.options = options;
            }

            public IEnumerable<KeyValuePair<string, object>> GetValues()
            {
                yield return new KeyValuePair<string, object>(nameof(options.MaxReceiveMessageSize), options.MaxReceiveMessageSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(options.MaxRetryAttempts), options.MaxRetryAttempts ?? -1);
                yield return new KeyValuePair<string, object>(nameof(options.MaxRetryBufferPerCallSize), options.MaxRetryBufferPerCallSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(options.MaxRetryBufferSize), options.MaxRetryBufferSize ?? -1);
                yield return new KeyValuePair<string, object>(nameof(options.MaxSendMessageSize), options.MaxSendMessageSize ?? -1);
            }
        }
    }
}
#endif
