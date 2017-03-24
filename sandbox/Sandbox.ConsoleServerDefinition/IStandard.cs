using MagicOnion;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
#if DEBUG
    public interface IStandard : IService<IStandard>
    {
        UnaryResult<int> Unary1(int x, int y);
        UnaryResult<int> Unary2(int x, int y);

        Task<ClientStreamingResult<int, string>> ClientStreaming1Async();
        Task<ServerStreamingResult<int>> ServerStreamingAsync(int x, int y, int z);
        Task<DuplexStreamingResult<int, int>> DuplexStreamingAsync();
    }
#endif
}
