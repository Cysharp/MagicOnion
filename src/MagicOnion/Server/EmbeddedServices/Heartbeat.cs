#if NON_UNITY

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

            return streaming.Result();
        }
    }
}

#endif