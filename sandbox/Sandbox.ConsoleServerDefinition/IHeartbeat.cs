using MagicOnion;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface IHeartbeat : IService<IHeartbeat>
    {
        Task<DuplexStreamingResult<Nil, Nil>> Connect();

        Task<UnaryResult<Nil>> TestSend(string connectionId);
    }
}
