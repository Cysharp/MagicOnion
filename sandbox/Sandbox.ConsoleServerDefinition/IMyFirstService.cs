using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    /// <summary>
    /// My first service definition.
    /// </summary>
    public interface IMyFirstService : IService<IMyFirstService>
    {
        /// <summary>
        /// The Sum Comment.
        /// </summary>
        /// <param name="x">My x, left value.</param>
        /// <param name="y">My y, right value.</param>
        /// <returns>everything.</returns>
        Task<UnaryResult<string>> SumAsync(int x, int y);
        UnaryResult<string> SumAsync2(int x, int y);

        Task<ClientStreamingResult<int, string>> StreamingOne();
        Task<ServerStreamingResult<string>> StreamingTwo(int x, int y, int z);
        ServerStreamingResult<string> StreamingTwo2(int x, int y, int z = 9999);

        Task<DuplexStreamingResult<int, string>> StreamingThree();
    }
}
