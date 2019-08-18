#pragma warning disable CS1998

using System;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;

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

    public class TestServiceWithDIImpl : ServiceBase<ITestService>, ITestService
    {
        readonly ILogger logger;

        public TestServiceWithDIImpl(ILogger<ITestService> logger)
        {
            this.logger = logger;
        }

        public async UnaryResult<int> Sum(int x, int y)
        {
            this.logger.LogDebug("{x} + {y}", x, y);
            return x + y;
        }
    }
}
#pragma warning restore CS1998
