using MagicOnion;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IStandard : IService<IStandard>
    {
        Task<UnaryResult<int>> Unary1Async(int x, int y);
        Task<ClientStreamingResult<int, string>> ClientStreaming1Async();
        Task<ServerStreamingResult<int>> ServerStreamingAsync(int x, int y, int z);
        Task<DuplexStreamingResult<int, int>> DuplexStreamingAsync();
    }
}
