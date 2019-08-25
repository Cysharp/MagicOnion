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

    public interface IFilterConstructorInjectionTester : IService<IFilterConstructorInjectionTester>
    {
        UnaryResult<int> A();
        UnaryResult<int> B();
    }

    [FromServiceFilter(typeof(ConstructorInjectedFilterAttribute))]
    public class FilterConstructorInjectionTester : ServiceBase<IFilterConstructorInjectionTester>, IFilterConstructorInjectionTester
    {
        public UnaryResult<int> A()
        {
            return UnaryResult(0);
        }

        [FromServiceFilter(typeof(ConstructorInjectedFilter2Attribute))]
        public UnaryResult<int> B()
        {
            return UnaryResult(0);
        }
    }
}
