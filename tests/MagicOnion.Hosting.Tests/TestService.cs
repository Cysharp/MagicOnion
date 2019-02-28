#pragma warning disable CS1998

using System;
using MagicOnion;
using MagicOnion.Server;

namespace MagicOnion.Hosting.Tests
{
    public interface ITestService : IService<ITestService>
    {
        UnaryResult<int> Sum(int x, int y);
    }
    public class TestServiceImpl : ServiceBase<ITestService>, ITestService
    {
        public async UnaryResult<int> Sum(int x, int y)
        {
            return x + y;
        }
    }
}
#pragma warning restore CS1998
