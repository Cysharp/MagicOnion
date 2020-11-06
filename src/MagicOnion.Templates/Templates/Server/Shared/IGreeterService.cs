using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;

namespace Company.MagicOnionServer1.Shared
{
    public interface IGreeterService : IService<IGreeterService>
    {
        UnaryResult<string> HelloAsync(string name);
    }
}
