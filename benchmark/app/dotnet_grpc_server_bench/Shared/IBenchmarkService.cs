using Benchmark.Server.Shared;
using Grpc.Core;
using MagicOnion;
using MessagePack;
using System.Threading.Tasks;

namespace Benchmark.Shared
{
    public interface IBenchmarkService : IService<IBenchmarkService>
    {
        UnaryResult<Nil> PlainTextAsync(BenchmarkData data);
    }
}
