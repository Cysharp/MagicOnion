using MessagePack;

namespace MagicOnion.Server.EmbeddedServices
{
    public interface IMagicOnionEmbeddedPing : IService<IMagicOnionEmbeddedPing>
    {
        UnaryResult<Nil> Ping();
    }

    [Ignore]
    internal class MagicOnionEmbeddedPing : ServiceBase<IMagicOnionEmbeddedPing>, IMagicOnionEmbeddedPing
    {
        public UnaryResult<Nil> Ping()
        {
            return UnaryResult(Nil.Default);
        }
    }
}