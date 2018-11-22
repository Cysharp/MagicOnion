using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Server.EmbeddedServices;
using MagicOnion.Utils;
using MessagePack;
using Sandbox.NetCoreServer.Hubs;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer
{
    class Program : IMessageReceiver
    {
        static async Task Main(string[] args)
        {
            const string GrpcHost = "localhost";

            Console.WriteLine("Server:::");

            //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");

            Environment.SetEnvironmentVariable("SETTINGS_MAX_HEADER_LIST_SIZE", "1000000");

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(new MagicOnionOptions(true)
            {
                // MagicOnionLogger = new MagicOnionLogToGrpcLogger(),
                MagicOnionLogger = new MagicOnionLogToGrpcLoggerWithNamedDataDump(),
                GlobalFilters = new MagicOnionFilterAttribute[]
                {
                },
                EnableCurrentContext = true,
                DisableEmbeddedService = true,
            });

            var server = new global::Grpc.Core.Server
            {
                Services = { service },
                Ports = { new ServerPort(GrpcHost, 12345, ServerCredentials.Insecure) }
            };

            server.Start();




            while (true)
            {
                var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
                //var hubCall = new ChatHubClient2(new DefaultCallInvoker(channel), null, default(CallOptions), null, null);

                //hubCall.__ConnectAndSubscribe(new Program());
                var hubCall = StreamingHubClient.Connect<IChatHub, IMessageReceiver>(new DefaultCallInvoker(channel), new Program(),null, default(CallOptions), null, null);



                await hubCall.EchoAsync("ほげほげ");
               var foooo = await hubCall.EchoRetrunAsync("ほげほげ");
               Console.WriteLine(foooo);
                Console.WriteLine("Press any key to stop.");
                Console.ReadLine();
            }
        }

        public Task OnReceiveMessage(int senderId, string message)
        {
            Console.WriteLine("Receive Here:" + senderId + ":" + message);

            return Task.CompletedTask;
        }
    }


    public class MyReceiver : IMessageReceiver
    {
        public Task OnReceiveMessage(int senderId, string message)
        {
            throw new NotImplementedException();
        }


        public Task OnReceiveMessage2(int senderId, string message)
        {

            return Task.CompletedTask;
            //            throw new NotImplementedException();
        }
    }


    public class ChatHubClient2 : StreamingHubClientBase<IChatHub, IMessageReceiver>, IChatHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IChatHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        public ChatHubClient2(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
            : base(callInvoker, host, option, resolver, logger)
        {
        }

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod => method;

        public Task EchoAsync(string message)
        {
            return WriteMessageAsync<string>(1297107480, message);
        }

        public Task<string> EchoRetrunAsync(string message)
        {
            return WriteMessageWithResponseAsync<string, string>(-1171618600, message);
        }

        protected override Task OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            if (methodId == 470021452) // OnReceiveMessage
            {
                var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string>>(data, resolver);
                return receiver.OnReceiveMessage(result.Item1, result.Item2);
            }

            return Task.CompletedTask;
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            if (methodId == -1171618600) // EchoReturnAsync
            {
                var result = LZ4MessagePackSerializer.Deserialize<string>(data, resolver);
                ((TaskCompletionSource<string>)taskCompletionSource).TrySetResult(result);
            }
        }
    }
}
