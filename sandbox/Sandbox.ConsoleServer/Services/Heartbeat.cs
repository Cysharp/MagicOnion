using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using SharedLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer.Services
{
    public class Heartbeat : ServiceBase<IHeartbeat>, IHeartbeat
    {
        public async Task<DuplexStreamingResult<Nil, Nil>> Connect()
        {
            var streaming = GetDuplexStreamingContext<Nil, Nil>();

            var id = ConnectionContext.GetConnectionId(Context);
            var cancellationTokenSource = ConnectionContext.Register(id);

            try
            {
                // wait client disconnect.
                // if client send complete event, safe unsubscribe of heartbeat.
                await streaming.MoveNext();
            }
            catch (RpcException)
            {
                // ok, cancelled.
            }
            finally
            {
                ConnectionContext.Unregister(id);
                cancellationTokenSource.Cancel();
            }

            return streaming.Result();
        }

        public Task<UnaryResult<Nil>> TestSend(string connectionId)
        {
            var connection = this.GetConnectionContext();

            connection.ConnectionStatus.Register(() =>
            {
                Console.WriteLine("Server disconnected detected!");
            });

            return Task.FromResult(UnaryResult(default(Nil)));
        }
    }
}

