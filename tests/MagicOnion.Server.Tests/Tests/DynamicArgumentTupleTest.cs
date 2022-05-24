using Grpc.Core;
using FluentAssertions;
using MagicOnion.Client;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Xunit;
using Xunit.Abstractions;

namespace MagicOnion.Server.Tests
{
    public interface IMaxArgCountExcessDynamicArgumentTupleService : IService<IMaxArgCountExcessDynamicArgumentTupleService>
    {
        // T0 - T15 (TypeParams = 16)
        UnaryResult<Nil> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15);
    }

    public class MaxArgCountExcessDynamicArgumentTupleService : ServiceBase<IMaxArgCountExcessDynamicArgumentTupleService>, IMaxArgCountExcessDynamicArgumentTupleService
    {
        public UnaryResult<Nil> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15)
        {
            return ReturnNil();
        }
    }

    public class MaxArgCountExcessDynamicArgumentTupleServiceTest : IClassFixture<ServerFixture<MaxArgCountExcessDynamicArgumentTupleService>>
    {
        ITestOutputHelper logger;
        GrpcChannel channel;

        public MaxArgCountExcessDynamicArgumentTupleServiceTest(ITestOutputHelper logger, ServerFixture<MaxArgCountExcessDynamicArgumentTupleService> server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task Unary1()
        {
            var ex = Assert.Throws<TypeInitializationException>(() => MagicOnionClient.Create<IMaxArgCountExcessDynamicArgumentTupleService>(channel));
            ex.InnerException.Should().BeOfType<InvalidOperationException>();
        }
    }

    public interface IDynamicArgumentTupleService : IService<IDynamicArgumentTupleService>
    {
        // T0 - T14 (TypeParams = 15)
        UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);
    }

    public class DynamicArgumentTupleService : ServiceBase<IDynamicArgumentTupleService>, IDynamicArgumentTupleService
    {
        public UnaryResult<int> Unary1(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14)
        {
            return UnaryResult(arg0 + arg1 + arg2 + arg3 + arg4 + arg5 + arg6 + arg7 + arg8 + arg9 + arg10 + arg11 + arg12 + arg13 + arg14);
        }
    }

    public class DynamicArgumentTupleServiceTest : IClassFixture<ServerFixture<DynamicArgumentTupleService>>
    {
        ITestOutputHelper logger;
        GrpcChannel channel;

        public DynamicArgumentTupleServiceTest(ITestOutputHelper logger, ServerFixture<DynamicArgumentTupleService> server)
        {
            this.logger = logger;
            this.channel = server.DefaultChannel;
        }

        [Fact]
        public async Task Unary1()
        {
            var client = MagicOnionClient.Create<IDynamicArgumentTupleService>(channel);
            var result  = await client.Unary1(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
            result.Should().Be(120);
        }
    }
}
