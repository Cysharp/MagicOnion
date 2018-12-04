#if NON_UNITY
#pragma warning disable CS0618 // Type or member is obsolete

using Grpc.Core;
using MessagePack;
using System.Threading.Tasks;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedHeartbeat : IService<IMagicOnionEmbeddedHeartbeat>
    {
        Task<DuplexStreamingResult<Nil, Nil>> Connect();
    }

    [Ignore]
    internal class MagicOnionEmbeddedHeartbeat : ServiceBase<IMagicOnionEmbeddedHeartbeat>, IMagicOnionEmbeddedHeartbeat
    {
        public async Task<DuplexStreamingResult<Nil, Nil>> Connect()
        {
            var streaming = GetDuplexStreamingContext<Nil, Nil>();

            var id = ConnectionContext.GetConnectionId(Context);
            var cancellationTokenSource = ConnectionContext.Register(id);

            try
            {
                // send to connect complete.
                await streaming.WriteAsync(Nil.Default).ConfigureAwait(false);

                // receive client hearbeat ping.
                while (await streaming.MoveNext().ConfigureAwait(false))
                {
                }
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

#endif

#pragma warning restore CS0618 // Type or member is obsolete