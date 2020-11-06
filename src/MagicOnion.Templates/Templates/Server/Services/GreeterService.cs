using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server;
using Company.MagicOnionServer1.Shared;

namespace Company.MagicOnionServer1.Services
{
    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
    {
        public async UnaryResult<string> HelloAsync(string name)
        {
            await Task.Delay(10);

            return $"Hello {name}!";
        }
    }
}
