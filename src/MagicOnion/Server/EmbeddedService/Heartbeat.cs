using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server.EmbeddedService
{
    internal interface IMagicOnionEmbeddedHeartbeat
    {
        Task<DuplexStreamingResult<bool, bool>> Connect();
    }

    [Ignore]
    internal class MagicOnionEmbeddedHeartbeat : ServiceBase<IMagicOnionEmbeddedHeartbeat>, IMagicOnionEmbeddedHeartbeat
    {
        public async Task<DuplexStreamingResult<bool, bool>> Connect()
        {
            var streaming = GetDuplexStreamingContext<bool, bool>();

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
    }
}