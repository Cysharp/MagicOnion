#if MAGICONION_USE_GRPC_CCORE
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.Unity
{
    public static partial class GrpcChannelProviderExtensions
    {
        /// <summary>
        /// Create a channel to the target and register the channel under the management of the provider.
        /// </summary>
        public static GrpcChannelx CreateChannel(this IGrpcChannelProvider provider, GrpcChannelTarget target, ChannelCredentials channelCredentials, IReadOnlyList<ChannelOption> channelOptions)
            => provider.CreateChannel(new CreateGrpcChannelContext(provider, target, new GrpcCCoreChannelOptions(channelOptions, channelCredentials)));
    }
}
#endif
