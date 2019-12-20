using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server;
using Xunit;
using Grpc.Core;
using MagicOnion.Client;
using Xunit.Abstractions;

namespace MagicOnion.Tests
{
    [Flags]
    public enum FilterCalledStatus
    {
        Begin = 1,
        Catch = 2,
        Finally = 4
    }

    public class SimpleFilter1 : MagicOnionFilterAttribute
    {
        public async override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                (context.Items["list"] as List<string>).Add(nameof(SimpleFilter1));
                context.Items[nameof(SimpleFilter1)] = FilterCalledStatus.Begin;
                await next(context);
            }
            catch
            {
                context.Items[nameof(SimpleFilter1)] = (FilterCalledStatus)context.Items[nameof(SimpleFilter1)] | FilterCalledStatus.Catch;
                throw;
            }
            finally
            {
                context.Items[nameof(SimpleFilter1)] = (FilterCalledStatus)context.Items[nameof(SimpleFilter1)] | FilterCalledStatus.Finally;
            }
        }
    }

    public class MultiFilter2 : MagicOnionFilterAttribute
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; set; }

        public MultiFilter2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                (context.Items["list"] as List<string>).Add(nameof(MultiFilter2));
                context.Items[nameof(MultiFilter2)] = FilterCalledStatus.Begin;
                context.Items[nameof(MultiFilter2) + "xyz"] = Tuple.Create(X, Y, Z);
                await next(context);
            }
            catch
            {
                context.Items[nameof(MultiFilter2)] = (FilterCalledStatus)context.Items[nameof(MultiFilter2)] | FilterCalledStatus.Catch;
                throw;
            }
            finally
            {
                context.Items[nameof(MultiFilter2)] = (FilterCalledStatus)context.Items[nameof(MultiFilter2)] | FilterCalledStatus.Finally;
            }
        }
    }

    public class MoreThanFilter3 : MagicOnionFilterAttribute
    {
        readonly string msg;

        public MoreThanFilter3(string msg)
        {
            this.msg = msg;
        }

        public async override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                (context.Items["list"] as List<string>).Add(nameof(MoreThanFilter3));
                context.Items[nameof(MoreThanFilter3)] = FilterCalledStatus.Begin;
                context.Items[nameof(MoreThanFilter3) + "msg"] = msg;
                await next(context);
            }
            catch
            {
                context.Items[nameof(MoreThanFilter3)] = (FilterCalledStatus)context.Items[nameof(MoreThanFilter3)] | FilterCalledStatus.Catch;
                throw;
            }
            finally
            {
                context.Items[nameof(MoreThanFilter3)] = (FilterCalledStatus)context.Items[nameof(MoreThanFilter3)] | FilterCalledStatus.Finally;
            }
        }
    }

    public class DumpFilter : MagicOnionFilterAttribute
    {
        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                context.Items["list"] = new List<string>();
                (context.Items["list"] as List<string>).Add(nameof(DumpFilter));
                await next(context);
            }
            catch
            {
            }
            finally
            {
                var calls = string.Join(", ", (context.Items["list"] as List<string>));
                var dict = string.Join(", ", context.Items.Where(x => x.Key != "list").OrderBy(x => x.Key).Select(x => x.Key + ", " + x.Value.ToString()));
                SetStatusCode(context, (StatusCode)999, calls + " : " + dict);
            }
        }
    }

    public interface IFilterTester : IService<IFilterTester>
    {
        UnaryResult<int> A();
        UnaryResult<int> B();
        UnaryResult<int> C();
    }

    [DumpFilter(Order = int.MinValue)]
    [MoreThanFilter3("put-class", Order = 244)]
    public class FilterTester : ServiceBase<IFilterTester>, IFilterTester
    {
        [SimpleFilter1(Order = 10)]
        public UnaryResult<int> A()
        {
            return UnaryResult(0);
        }

        [SimpleFilter1(Order = 300)]
        [MultiFilter2(99, 30, Z = 4595, Order = 200)]
        public UnaryResult<int> B()
        {
            return UnaryResult(999);
        }

        [SimpleFilter1(Order = int.MinValue)]
        public UnaryResult<int> C()
        {
            throw new Exception("C-Exception");
        }
    }
    
    [CollectionDefinition(nameof(FilterTestCollectionServerFixture))]
    public class FilterTestCollectionServerFixture : ICollectionFixture<FilterTestCollectionServerFixture.CustomServerFixture>
    {
        public class CustomServerFixture : ServerFixture
        {
            protected override MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
            {
                return MagicOnionEngine.BuildServerServiceDefinition(new[] { typeof(FilterTester) }, options);
            }
        }
    }

    [Collection(nameof(FilterTestCollectionServerFixture))]
    public class FilterTest
    {
        IFilterTester client;

        public FilterTest(ITestOutputHelper logger, FilterTestCollectionServerFixture.CustomServerFixture server)
        {
            this.client = server.CreateClient<IFilterTester>();
        }


        [Fact]
        public void Filter()
        {
            Assert.Throws<RpcException>(() => client.A().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("DumpFilter, SimpleFilter1, MoreThanFilter3 : MoreThanFilter3, Begin, Finally, MoreThanFilter3msg, put-class, SimpleFilter1, Begin, Finally");

            Assert.Throws<RpcException>(() => client.B().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("DumpFilter, MultiFilter2, MoreThanFilter3, SimpleFilter1 : MoreThanFilter3, Begin, Finally, MoreThanFilter3msg, put-class, MultiFilter2, Begin, Finally, MultiFilter2xyz, (99, 30, 4595), SimpleFilter1, Begin, Finally");

            Assert.Throws<RpcException>(() => client.C().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("DumpFilter, SimpleFilter1, MoreThanFilter3 : MoreThanFilter3, Begin, Catch, Finally, MoreThanFilter3msg, put-class, SimpleFilter1, Begin, Catch, Finally");
        }
    }
}
