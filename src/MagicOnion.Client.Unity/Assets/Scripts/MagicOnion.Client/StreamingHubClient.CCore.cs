#if !NON_UNITY || NET461 || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        [Obsolete]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(Channel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(new DefaultCallInvoker(channel), receiver, host, option, serializerOptions, logger);
        }

        public static Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(Channel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return ConnectAsync<TStreamingHub, TReceiver>(new DefaultCallInvoker(channel), receiver, host, option, serializerOptions, logger);
        }
    }
}
#endif