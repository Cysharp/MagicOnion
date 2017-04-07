using MagicOnion;
using SharedLibrary;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IStandard : IService<IStandard>
    {
        UnaryResult<int?> NullableCheck(bool isNull);

        UnaryResult<int> Unary1(int x, int y);
        UnaryResult<int> Unary2(int x, int y);

        Task<ClientStreamingResult<int, string>> ClientStreaming1Async();
        Task<ServerStreamingResult<int>> ServerStreamingAsync(int x, int y, int z);
        Task<DuplexStreamingResult<int, int>> DuplexStreamingAsync();

        UnaryResult<MyClass2> Echo(string name, int x, int y, MyEnum2 e);
    }
}
