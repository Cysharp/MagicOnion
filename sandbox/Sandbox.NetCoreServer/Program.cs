using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.HttpGateway.Swagger;
using MagicOnion.Server;
using MagicOnion.Server.EmbeddedServices;
using MagicOnion.Utils;
using MessagePack;
using MagicOnion.Hosting;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sandbox.NetCoreServer.Hubs;
using Sandbox.NetCoreServer.Services;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // ServiceLocatorHelper.CreateService<UnaryService, ICalcSerivce>(null);

            const string GrpcHost = "localhost";

            Console.WriteLine("Server:::");

            //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");

            Environment.SetEnvironmentVariable("SETTINGS_MAX_HEADER_LIST_SIZE", "1000000");

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var options = new MagicOnionOptions(true)
            {
                //MagicOnionLogger = new MagicOnionLogToGrpcLogger(),
                MagicOnionLogger = new MagicOnionLogToGrpcLoggerWithNamedDataDump(),
                GlobalFilters = new MagicOnionFilterAttribute[]
                {
                },
                EnableCurrentContext = true,
                DisableEmbeddedService = true,
            };


            var magicOnionHost = MagicOnionHost.CreateDefaultBuilder(useSimpleConsoleLogger: true)
                .UseMagicOnion(options, new ServerPort("localhost", 12345, ServerCredentials.Insecure))
                .UseConsoleLifetime()
                .Build();

            // test webhost

            // NuGet: Microsoft.AspNetCore.Server.Kestrel
            var webHost = new WebHostBuilder()
                .ConfigureServices(collection =>
                {
                    // Add MagicOnionServiceDefinition for reference from Startup.
                    collection.AddSingleton<MagicOnionServiceDefinition>(magicOnionHost.Services.GetService<MagicOnionHostedServiceDefinition>().ServiceDefinition);
                })
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5432")
                .Build();

            await Task.WhenAll(webHost.RunAsync(), magicOnionHost.RunAsync());


            //webHost.Run();

            //Console.ReadLine();


            //{
            //    var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
            //    var client = StreamingHubClient.Connect<IChatHub, IMessageReceiver2>(channel, new Receiver());

            //    Console.WriteLine("Call to Server");
            //    await client.JoinAsync("me", "foo");
            //    await Task.WhenAll(
            //        client.SendMessageAsync("a"),
            //        client.SendMessageAsync("b"),
            //        client.SendMessageAsync("c"),
            //        client.SendMessageAsync("d"),
            //        client.SendMessageAsync("e"));

            //    Console.WriteLine("OK to Send");
            //    await client.DisposeAsync();
            //    await channel.ShutdownAsync();
            //}


        }

        static void CheckUnsfeResolver()
        {
            UnsafeDirectBlitResolver.Register<Foo>();
            CompositeResolver.RegisterAndSetAsDefault(
                UnsafeDirectBlitResolver.Instance,

                BuiltinResolver.Instance

                );

            var f = new Foo { A = 10, B = 9999, C = 9999999 };
            var doudarou = MessagePackSerializer.Serialize(f, UnsafeDirectBlitResolver.Instance);
            var two = MessagePackSerializer.Deserialize<Foo>(doudarou);


            var f2 = new[]{
                new Foo { A = 10, B = 9999, C = 9999999 },
                new Foo { A = 101, B = 43, C = 234 },
                new Foo { A = 20, B = 5666, C = 1111 },
            };
            var doudarou2 = MessagePackSerializer.Serialize(f2, UnsafeDirectBlitResolver.Instance);
            var two2 = MessagePackSerializer.Deserialize<Foo[]>(doudarou2);


            Console.WriteLine(string.Join(", ", doudarou2));
        }
    }

    public class Startup
    {
        // Inject MagicOnionServiceDefinition from DIl
        public void Configure(IApplicationBuilder app, MagicOnionServiceDefinition magicOnion)
        {
            // Optional:Summary to Swagger
            var xmlName = "Sandbox.NetCoreServer.xml";
            var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

            // HttpGateway has two middlewares.
            // One is SwaggerView(MagicOnionSwaggerMiddleware)
            // One is Http1-JSON to gRPC-MagicOnion gateway(MagicOnionHttpGateway)
            app.UseMagicOnionSwagger(magicOnion.MethodHandlers, new SwaggerOptions("MagicOnion.Server", "Swagger Integration Test", "/")
            {
                XmlDocumentPath = xmlPath
            });
            app.UseMagicOnionHttpGateway(magicOnion.MethodHandlers, new Channel("localhost:12345", ChannelCredentials.Insecure));
        }
    }

    public struct Foo
    {
        public byte A;
        public long B;
        public int C;
    }

    class Receiver : IMessageReceiver2
    {
        public void Foo2(Foo foo2)
        {
            throw new NotImplementedException();
        }

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
