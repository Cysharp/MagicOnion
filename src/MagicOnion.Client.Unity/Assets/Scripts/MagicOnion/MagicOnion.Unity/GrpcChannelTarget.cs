using System.Collections;
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.Unity
{
    /// <summary>
    /// Represents gRPC channel target.
    /// </summary>
    public readonly struct GrpcChannelTarget
    {
        public string Host { get; }
        public int Port { get; }
        public ChannelCredentials ChannelCredentials { get; }

        public GrpcChannelTarget(string host, int port, ChannelCredentials channelCredentials)
        {
            Host = host;
            Port = port;
            ChannelCredentials = channelCredentials;
        }
    }
}