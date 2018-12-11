using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Server.EmbeddedServices;
using MagicOnion.Utils;
using MessagePack;
using Sandbox.NetCoreServer.Hubs;
using Sandbox.NetCoreServer.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer
{
    class Program
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

            {
                var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
                var client = StreamingHubClient.Connect<IChatHub, IMessageReceiver2>(channel, new Receiver());

                Console.WriteLine("Call to Server");
                await client.JoinAsync("me", "foo");
                await Task.WhenAll(
                    client.SendMessageAsync("a"),
                    client.SendMessageAsync("b"),
                    client.SendMessageAsync("c"),
                    client.SendMessageAsync("d"),
                    client.SendMessageAsync("e"));

                Console.WriteLine("OK to Send");
            }
        }

    }

    class Receiver : IMessageReceiver2
    {
        public void OnReceiveMessage(string senderUser, string message)
        {
            Console.WriteLine(senderUser + ":" + message);
        }
    }


    //    public class ClientProgram : 

    //    {
    //        public async Task Start(string user, string room)
    //        {
    //            var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);

    //            var client = StreamingHubClient.Connect<IChatHub, IMessageReceiver>(channel, this);
    //            RegisterDisconnect(client);
    //            try
    //            {
    //                await client.JoinAsync(user, room);

    //                await client.SendMessageAsync("Who");
    //                await client.SendMessageAsync("Bar");
    //                await client.SendMessageAsync("Baz");

    //                await client.LeaveAsync();
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex);
    //            }
    //            finally
    //            {
    //                await client.DisposeAsync();
    //            }
    //        }

    //        async void RegisterDisconnect(IChatHub client)
    //        {
    //            try
    //            {
    //                // you can wait disconnected event
    //                await client.WaitForDisconnect();
    //            }
    //            finally
    //            {
    //                // try-to-reconnect? logging event? etc...
    //                Console.WriteLine("disconnected");
    //            }
    //        }

    //#pragma warning disable CS1998

    //        public void OnReceiveMessage(string senderUser, string message)
    //        {
    //            Console.WriteLine(senderUser + ":" + message);
    //        }
    //    }




    //public class MyReceiver : IMessageReceiver
    //{
    //    public Task OnReceiveMessage(int senderId, string message)
    //    {
    //        throw new NotImplementedException();
    //    }


    //    public Task OnReceiveMessage2(int senderId, string message)
    //    {

    //        return Task.CompletedTask;
    //        //            throw new NotImplementedException();
    //    }
    //}


    //public class ChatHubClient2 : StreamingHubClientBase<IChatHub, IMessageReceiver>, IChatHub
    //{
    //    static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IChatHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

    //    public ChatHubClient2(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger)
    //        : base(callInvoker, host, option, resolver, logger)
    //    {
    //    }

    //    protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod => method;


    //    public Task EchoAsync(string message)
    //    {
    //        return WriteMessageAsync<string>(1297107480, message);
    //    }

    //    public Task<string> EchoRetrunAsync(string message)
    //    {
    //        return WriteMessageWithResponseAsync<string, string>(-1171618600, message);
    //    }

    //    public IChatHub FireAndForget()
    //    {
    //        return new FireAndForgetClient(this); 
    //    }

    //    protected override Task OnBroadcastEvent(int methodId, ArraySegment<byte> data)
    //    {
    //        if (methodId == 470021452) // OnReceiveMessage
    //        {
    //            var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string>>(data, resolver);
    //            return receiver.OnReceiveMessage(result.Item1, result.Item2);
    //        }

    //        return Task.CompletedTask;
    //    }

    //    protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
    //    {
    //        if (methodId == -1171618600) // EchoReturnAsync
    //        {
    //            var result = LZ4MessagePackSerializer.Deserialize<string>(data, resolver);
    //            ((TaskCompletionSource<string>)taskCompletionSource).TrySetResult(result);
    //        }
    //    }

    //    class FireAndForgetClient :Object, IChatHub
    //    {
    //        ChatHubClient2 client;

    //        public FireAndForgetClient(ChatHubClient2 client)
    //        {
    //            this.client = client; 
    //        }

    //        public Task DisposeAsync()
    //        {
    //            throw new NotSupportedException();
    //        }

    //        public IChatHub FireAndForget()
    //        {
    //            throw new NotSupportedException();
    //        }

    //        public Task WaitForDisconnect()
    //        {
    //            throw new NotSupportedException();
    //        }

    //        public Task<Nil> JoinAsync(string userName, string roomName)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Task<Nil> LeaveAsync()
    //        {
    //            return client.WriteMessageAsyncFireAndForget<Nil, Nil>(1297107480, Nil.Default);
    //        }

    //        public Task SendMessageAsync(string message)
    //        {
    //            return client.WriteMessageAsync(1297107480, message);
    //        }
    //    }
    //}
}
