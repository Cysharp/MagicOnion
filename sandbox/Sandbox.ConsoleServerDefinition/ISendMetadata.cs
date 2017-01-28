using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface ISendMetadata : IService<ISendMetadata>
    {
        Task<UnaryResult<int>> PangPong();
    }
}
