#if NON_UNITY && !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MessagePack;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        [Obsolete]
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(GrpcChannel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, serializerOptions, logger);
        }
        
        public static Task<TStreamingHub> ConnectAsync<TStreamingHub, TReceiver>(GrpcChannel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return ConnectAsync<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, serializerOptions, logger);
        }
    }
}
#endif