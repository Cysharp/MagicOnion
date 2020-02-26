using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;

namespace JwtAuthApp.Shared
{
    public interface IGreeterService : IService<IGreeterService>
    {
        UnaryResult<string> HelloAsync();
    }
}
