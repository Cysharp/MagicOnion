using System;
using System.Collections.Generic;
#if USE_GRPC_CCORE
using Grpc.Core;
#else
using Grpc.Net.Client;
#endif
using MagicOnion.Client;

namespace MagicOnion.Unity
{
    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
#if USE_GRPC_CCORE
    public class DefaultGrpcChannelProvider : GrpcCCoreGrpcChannelProvider
    {
        public DefaultGrpcChannelProvider() : base() { }
        public DefaultGrpcChannelProvider(IReadOnlyList<ChannelOption> channelOptions) : base(channelOptions) { }
        public DefaultGrpcChannelProvider(GrpcCCoreChannelOptions channelOptions) : base(channelOptions) { }
    }
#else
    public class DefaultGrpcChannelProvider : GrpcNetClientGrpcChannelProvider
    {
        public DefaultGrpcChannelProvider() : base() {}
        public DefaultGrpcChannelProvider(GrpcChannelOptions channelOptions) : base(channelOptions) {}
        public DefaultGrpcChannelProvider(Func<GrpcChannelOptions> channelOptionsFactory) : base(channelOptionsFactory) {}
    }
#endif
}
