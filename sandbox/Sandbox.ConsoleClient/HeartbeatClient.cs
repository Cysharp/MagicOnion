using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using Sandbox.ConsoleServer;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.ConsoleClient
{
    public static class HearbeatClient
    {
        public static async Task Test(Channel channel)
        {
            var manager = await ClientConnectionLifetimeManager.Connect(channel);
            manager.RegisterDisconnectAction(() =>
            {
                Console.WriteLine("disconnected!");
            });

            await MagicOnionClient.Create<IHeartbeat>(channel).TestSend(manager.ConnectionId);
        }
    }


    public class ClientConnectionLifetimeManager
    {
        public const string HeaderKey = "connection_id";

        readonly Task connectiongTask;
        readonly CancellationTokenSource source;
        readonly DuplexStreamingResult<Nil, Nil> method;
        readonly string connectionId;

        public string ConnectionId { get { return connectionId; } }

        public bool IsConnecting
        {
            get
            {
                return !source.IsCancellationRequested;
            }
        }

        public ClientConnectionLifetimeManager(Task connectiongTask, CancellationTokenSource source, string connectionId, DuplexStreamingResult<Nil, Nil> method)
        {
            this.connectiongTask = connectiongTask;
            this.source = source;
            this.connectionId = connectionId;
            this.method = method;
        }

        public static async Task<ClientConnectionLifetimeManager> Connect(Channel channel, Metadata metadata = null)
        {
            var connectionId = Guid.NewGuid().ToString();
            metadata = metadata ?? new Metadata();
            metadata.Add(HeaderKey, connectionId);

            var source = new CancellationTokenSource();
            var method = await MagicOnionClient.Create<IHeartbeat>(channel)
               .WithHeaders(metadata)
               .Connect();

            // 
            var task = method.ResponseStream.MoveNext()
                .ContinueWith((x, state) =>
                {
                    // both okay for success or failer
                    ((CancellationTokenSource)state).Cancel();
                }, source);

            return new ClientConnectionLifetimeManager(task, source, connectionId, method);
        }

        public CancellationTokenRegistration RegisterDisconnectAction(Action action)
        {
            return source.Token.Register(action);
        }

        public void Complete()
        {
            if (!source.IsCancellationRequested)
            {
                method.RequestStream.CompleteAsync().Wait();
            }
            source.Cancel();
            connectiongTask.Dispose();
        }
    }
}
