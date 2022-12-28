using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;

namespace JwtAuthApp.Shared
{
    public interface IGreeterService : IService<IGreeterService>
    {
        UnaryResult<string> HelloAsync();
        Task<ServerStreamingResult<string>> ServerAsync(string name, int age);
        Task<ClientStreamingResult<int, string>> ClientAsync();
        Task<DuplexStreamingResult<int, string>> DuplexAsync();
    }
}
