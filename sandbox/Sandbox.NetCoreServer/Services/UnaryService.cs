#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using MagicOnion;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer.Services
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<string> SumAsync(int x, int y);
    }

    public class UnaryService : ServiceBase<IMyFirstService>, IMyFirstService
    {
        [MyFirstFilter]
        public async UnaryResult<string> SumAsync(int x, int y)
        {
            return (x + y).ToString();
        }
    }

    public class MyFirstFilter : MagicOnionFilterAttribute
    {
        public MyFirstFilter()
            : base(null)
        {

        }

        public MyFirstFilter(Func<ServiceContext, ValueTask> next)
            : base(next)
        {
        }

        public override ValueTask Invoke(ServiceContext context)
        {
            try
            {
                Console.WriteLine("BF");
                return Next(context);
            }
            finally
            {
                Console.WriteLine("AF");
            }
        }
    }
}

