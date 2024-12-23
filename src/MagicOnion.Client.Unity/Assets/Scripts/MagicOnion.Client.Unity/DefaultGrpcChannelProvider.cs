using System;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace MagicOnion.Unity
{
    /// <summary>
    /// Provide and manage gRPC channels for MagicOnion.
    /// </summary>
    public class DefaultGrpcChannelProvider : GrpcNetClientGrpcChannelProvider
    {
        public DefaultGrpcChannelProvider() : base() {}
        [Obsolete("Use constructor with a GrpcChannelOptions factory overload instead. If you pass a GrpcChannelOptions directly, HttpClient/HttpHandler may be reused unintentionally.")]
        public DefaultGrpcChannelProvider(GrpcChannelOptions channelOptions) : base(channelOptions) { }
        public DefaultGrpcChannelProvider(Func<GrpcChannelOptions> channelOptionsFactory) : base(channelOptionsFactory) {}
    }
}
