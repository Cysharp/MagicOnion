using System;
using System.Collections.Generic;
#if MAGICONION_USE_GRPC_CCORE
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
#if MAGICONION_USE_GRPC_CCORE
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
        [Obsolete("Use constructor with a GrpcChannelOptions factory overload instead. If you pass a GrpcChannelOptions directly, HttpClient/HttpHandler may be reused unintentionally.")]
        public DefaultGrpcChannelProvider(GrpcChannelOptions channelOptions) : base(channelOptions) { }
        public DefaultGrpcChannelProvider(Func<GrpcChannelOptions> channelOptionsFactory) : base(channelOptionsFactory) {}
    }
#endif
}
