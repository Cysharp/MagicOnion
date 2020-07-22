#if !NON_UNITY || USE_GRPC_CCORE
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(Channel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(new DefaultCallInvoker(channel), receiver, host, option, serializerOptions, logger);
        }
    }
}
#endif