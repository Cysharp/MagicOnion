using Grpc.Core;
using MagicOnion.Server;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Hosting
{
    sealed class MagicOnionServerService : IHostedService
    {
        readonly MagicOnionServiceDefinition serviceDefinition;
        readonly IEnumerable<ServerPort> ports;
        readonly IEnumerable<ChannelOption> channelOptions;

        global::Grpc.Core.Server server;

        public MagicOnionServerService(MagicOnionServiceDefinition serviceDefinition, IEnumerable<ServerPort> ports, IEnumerable<ChannelOption> channelOptions)
        {
            this.serviceDefinition = serviceDefinition;
            this.ports = ports;
            this.channelOptions = channelOptions;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartTask(cancellationToken);
            return Task.CompletedTask;
        }

        void StartTask(CancellationToken token)
        {
            if (server != null)
            {
                // already running
                return;
            }
            var newServer = new global::Grpc.Core.Server(channelOptions)
            {
                Services = { serviceDefinition },
            };
            // if another server is set in another thread, just leave it.
            if (null == Interlocked.CompareExchange(ref server, newServer, null))
            {
                foreach (var port in ports)
                {
                    server.Ports.Add(port);
                }
                server.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            do
            {
                var tmp = server;
                if (tmp == Interlocked.CompareExchange(ref server, null, tmp))
                {
                    if (tmp != null)
                    {
                        await tmp.ShutdownAsync().ConfigureAwait(false);
                    }
                }
            } while (server != null);
        }
    }

}
