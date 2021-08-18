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
        public bool IsInsecure { get; }

        public GrpcChannelTarget(string host, int port, bool isInsecure)
        {
            Host = host;
            Port = port;
            IsInsecure = isInsecure;
        }
    }
}