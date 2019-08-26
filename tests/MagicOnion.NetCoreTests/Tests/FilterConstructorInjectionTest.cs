using FluentAssertions;
using MagicOnion.Server;
using Xunit;
using Grpc.Core;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System;

namespace MagicOnion.Tests
{
    [CollectionDefinition(nameof(FilterConstructorInjectionTestGrpcServerFixture))]
    public class FilterConstructorInjectionTestGrpcServerFixture : ICollectionFixture<FilterConstructorInjectionTestGrpcServerFixture.CustomServerFixture>
    {
        public class CustomServerFixture : ServerFixture
        {
            protected override void PrepareServer()
            {
                DefaultServiceLocator.Instance.Register(new FilterConstructorInjectionValue());
                base.PrepareServer();
            }
        }
    }

    [Collection(nameof(FilterConstructorInjectionTestGrpcServerFixture))]
    public class FilterConstructorInjectionTest
    {
        IFilterConstructorInjectionTester client;

        public FilterConstructorInjectionTest(ITestOutputHelper logger, FilterConstructorInjectionTestGrpcServerFixture.CustomServerFixture server)
        {
            this.client = server.CreateClient<IFilterConstructorInjectionTester>();
        }

        [Fact]
        public void Filter()
        {
            Assert.Throws<RpcException>(() => client.A().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("ConstructorInjectedFilterAttribute");
            Assert.Throws<RpcException>(() => client.B().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("ConstructorInjectedFilterAttributeConstructorInjectedFilter2Attribute");
            Assert.Throws<RpcException>(() => client.C().GetAwaiter().GetResult()).Status.Detail
                .Should().Be("ConstructorInjectedFilterAttributeConstructorInjectedFilter3Attributefoo987654");
        }
    }


    public class FilterConstructorInjectionValue { }

    public class ConstructorInjectedFilterAttribute : MagicOnionFilterAttribute
    {
        readonly FilterConstructorInjectionValue injected;

        public ConstructorInjectedFilterAttribute(FilterConstructorInjectionValue injected)
        {
            this.injected = injected;
        }

        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            var prevDetail = context.CallContext?.Status.Detail ?? string.Empty;
            context.CallContext.Status = new Grpc.Core.Status(StatusCode.Unknown, prevDetail + (this.injected != null ? nameof(ConstructorInjectedFilterAttribute) : ""));
            return next(context);
        }
    }

    public class ConstructorInjectedFilter2Attribute : MagicOnionFilterAttribute
    {
        readonly FilterConstructorInjectionValue injected;

        public ConstructorInjectedFilter2Attribute(FilterConstructorInjectionValue injected)
        {
            this.injected = injected;
        }

        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            var prevDetail = context.CallContext?.Status.Detail ?? string.Empty;
            context.CallContext.Status = new Grpc.Core.Status(StatusCode.Unknown, prevDetail + (this.injected != null ? nameof(ConstructorInjectedFilter2Attribute) : ""));
            return next(context);
        }
    }

    public class ConstructorInjectedFilter3Attribute : MagicOnionFilterAttribute
    {
        readonly FilterConstructorInjectionValue injected;
        readonly string arg1;
        readonly int arg2;

        public ConstructorInjectedFilter3Attribute(string arg1, int arg2, FilterConstructorInjectionValue injected)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.injected = injected;
        }

        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            var prevDetail = context.CallContext?.Status.Detail ?? string.Empty;
            context.CallContext.Status = new Grpc.Core.Status(StatusCode.Unknown, prevDetail + (this.injected != null ? nameof(ConstructorInjectedFilter3Attribute) + this.arg1 + this.arg2 : ""));
            return next(context);
        }
    }

    public interface IFilterConstructorInjectionTester : IService<IFilterConstructorInjectionTester>
    {
        UnaryResult<int> A();
        UnaryResult<int> B();
        UnaryResult<int> C();
    }

    [FromTypeFilter(typeof(ConstructorInjectedFilterAttribute))]
    public class FilterConstructorInjectionTester : ServiceBase<IFilterConstructorInjectionTester>, IFilterConstructorInjectionTester
    {
        public UnaryResult<int> A()
        {
            return UnaryResult(0);
        }

        [FromTypeFilter(typeof(ConstructorInjectedFilter2Attribute))]
        public UnaryResult<int> B()
        {
            return UnaryResult(0);
        }

        [FromTypeFilter(typeof(ConstructorInjectedFilter3Attribute), Arguments = new object[] { "foo", 987654 })]
        public UnaryResult<int> C()
        {
            return UnaryResult(0);
        }
    }
}
