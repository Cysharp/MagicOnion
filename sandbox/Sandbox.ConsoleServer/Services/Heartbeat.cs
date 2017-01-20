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

namespace MagicOnion.Server
{
    public class ConnectionContext
    {
        #region instance

        ConcurrentDictionary<string, object> items;

        /// <summary>Object storage per invoke.</summary>
        public ConcurrentDictionary<string, object> Items
        {
            get
            {
                if (items == null) items = new ConcurrentDictionary<string, object>();
                return items;
            }
        }

        public string ConnectionId { get; }

        public CancellationToken ConnectionStatus { get; }

        public ConnectionContext(string connectionId, CancellationToken connectionStatus)
        {
            this.ConnectionId = connectionId;
            this.ConnectionStatus = connectionStatus;
        }

        #endregion

        #region manager

        // Factory and Cache.

        public const string HeaderKey = "connection_id";
        public static ConcurrentDictionary<string, ConnectionContext> manager = new ConcurrentDictionary<string, ConnectionContext>();

        public static string GetConnectionId(ServiceContext context)
        {
            var connectionId = context.CallContext.RequestHeaders.Get(HeaderKey);
            if (connectionId == null || connectionId.IsBinary) throw new Exception("ConnectionLifetimeManager must needs `ConnId` header and Guid string.");

            return connectionId.Value;
        }

        public static CancellationTokenSource Register(string connectionId)
        {
            var cts = new CancellationTokenSource();
            manager[connectionId] = new ConnectionContext(connectionId, cts.Token);
            return cts;
        }

        public static void Unregister(string connectionId)
        {
            ConnectionContext _;
            manager.TryRemove(connectionId, out _);
        }

        public static ConnectionContext GetContext(string connectionId)
        {
            ConnectionContext source;
            if (manager.TryGetValue(connectionId, out source))
            {
                return source;
            }
            else
            {
                throw new Exception("Heartbeat connection doesn't registered.");
            }
        }


        #endregion
    }

    public static class ConnectionContextExtensions
    {
        public static ConnectionContext GetConnectionContext<T>(this ServiceBase<T> service)
        {
            return service.Context.GetConnectionContext();
        }

        public static ConnectionContext GetConnectionContext(this ServiceContext context)
        {
            var id = ConnectionContext.GetConnectionId(context);
            return ConnectionContext.GetContext(id);
        }
    }
}
