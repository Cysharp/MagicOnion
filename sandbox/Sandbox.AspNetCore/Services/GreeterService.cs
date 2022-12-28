using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server;

namespace Sandbox.AspNetCore.Services
{
    public interface IGreeterService : IService<IGreeterService>
    {
        UnaryResult<string> HelloAsync();
    }

    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
    {
        public UnaryResult<string> HelloAsync()
        {
            return UnaryResult.FromResult("Konnichiwa from MagicOnion + ASP.NET Core");
        }
    }
}
