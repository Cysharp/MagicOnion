#if MAGICONION_USE_GRPC_CCORE
using System;
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.Unity
{
    /// <summary>
    /// Provide and manage gRPC channels using gRPC C-core for MagicOnion.
    /// </summary>
    public class GrpcCCoreGrpcChannelProvider : GrpcChannelProviderBase
    {
        private readonly GrpcCCoreChannelOptions defaultChannelOptions;

        public GrpcCCoreGrpcChannelProvider()
            : this(new GrpcCCoreChannelOptions())
        {
        }

        public GrpcCCoreGrpcChannelProvider(IReadOnlyList<ChannelOption> channelOptions)
            : this(new GrpcCCoreChannelOptions(channelOptions))
        {
        }

        public GrpcCCoreGrpcChannelProvider(GrpcCCoreChannelOptions channelOptions)
        {
            defaultChannelOptions = channelOptions ?? throw new ArgumentNullException(nameof(channelOptions));
        }

        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        protected override GrpcChannelx CreateChannelCore(int id, CreateGrpcChannelContext context)
        {
            var channelOptions = context.ChannelOptions.GetOrDefault<GrpcCCoreChannelOptions>() ?? defaultChannelOptions;
            var channel = new Channel(context.Target.Host, context.Target.Port, context.Target.IsInsecure ? ChannelCredentials.Insecure : channelOptions.ChannelCredentials, channelOptions.ChannelOptions);
            var channelHolder = new GrpcChannelx(
                id,
                context.Provider.UnregisterChannel /* Root provider may be wrapped outside this provider class. */,
                channel,
                new Uri((context.Target.IsInsecure ? "http" : "https") + $"://{context.Target.Host}:{context.Target.Port}"),
                new GrpcChannelOptionsBag(channelOptions)
            );

            return channelHolder;
        }
    }

    public sealed class GrpcCCoreChannelOptions : IChannelOptionsValueProvider
    {
        public IReadOnlyList<ChannelOption> ChannelOptions { get; set; }
        public ChannelCredentials ChannelCredentials { get; set; }

        public GrpcCCoreChannelOptions(IReadOnlyList<ChannelOption>? channelOptions = null, ChannelCredentials? channelCredentials = null)
        {
            ChannelOptions = channelOptions ?? Array.Empty<ChannelOption>();
            ChannelCredentials = channelCredentials ?? new SslCredentials();
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            foreach (var channelOption in ChannelOptions)
            {
                yield return new KeyValuePair<string, object>(channelOption.Name, channelOption.Type == ChannelOption.OptionType.Integer ? channelOption.IntValue : channelOption.StringValue);
            }
        }
    }
}
#endif
