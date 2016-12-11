using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        Task<UnaryResult<string>> SumAsync(int x, int y);
        UnaryResult<string> SumAsync2(int x, int y);

        Task<ClientStreamingResult<int, string>> StreamingOne();
        Task<ServerStreamingResult<string>> StreamingTwo(int x, int y, int z);
        ServerStreamingResult<string> StreamingTwo2(int x, int y, int z);
    }
}
