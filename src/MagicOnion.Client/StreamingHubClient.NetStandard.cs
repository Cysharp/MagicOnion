#if NON_UNITY
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using MessagePack;

namespace MagicOnion.Client
{
    public static partial class StreamingHubClient
    {
        public static TStreamingHub Connect<TStreamingHub, TReceiver>(GrpcChannel channel, TReceiver receiver, string host = null, CallOptions option = default(CallOptions), MessagePackSerializerOptions serializerOptions = null, IMagicOnionClientLogger logger = null)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            return Connect<TStreamingHub, TReceiver>(channel.CreateCallInvoker(), receiver, host, option, serializerOptions, logger);
        }
    }
}
#endif