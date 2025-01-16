using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using MagicOnion.Server.Filters;

namespace MagicOnion.Server.Tests;

[CollectionDefinition(nameof(FilterConstructorInjectionTestGrpcServerFixture))]
public class FilterConstructorInjectionTestGrpcServerFixture : ICollectionFixture<FilterConstructorInjectionTestGrpcServerFixture.CustomServerFixture>
{
    public class CustomServerFixture : ServerFixture<FilterConstructorInjectionTester>
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<FilterConstructorInjectionValue>();
            services.AddSingleton<ServiceFilterForMethodTestFilterAttribute>();
        }

        protected override void ConfigureMagicOnion(MagicOnionOptions options)
        {
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
    public void TypeFilterForClassTest()
    {
        var ex = Assert.Throws<RpcException>(() => client.A().GetAwaiter().GetResult());
        Assert.Equal("ConstructorInjectedFilterAttribute", ex.Status.Detail);
    }

    [Fact]
    public void TypeFilterForMethodTest()
    {
        var ex = Assert.Throws<RpcException>(() => client.A().GetAwaiter().GetResult());
        Assert.Equal("ConstructorInjectedFilterAttribute", ex.Status.Detail);
    }

    [Fact]
    public void TypeFilterWithArgumentsForMethodTest()
    {
        var ex = Assert.Throws<RpcException>(() => client.C().GetAwaiter().GetResult());
        Assert.Equal("ConstructorInjectedFilterAttributeConstructorInjectedFilter3Attributefoo987654", ex.Status.Detail);
    }

    [Fact]
    public void ServiceFilterForMethodTest()
    {
        var ex = Assert.Throws<RpcException>(() => client.D().GetAwaiter().GetResult());
        Assert.Equal("ConstructorInjectedFilterAttributeServiceFilterForMethodTestFilterAttribute", ex.Status.Detail);
    }

    [Fact]
    public void FilterFactoryTest()
    {
        var ex = Assert.Throws<RpcException>(() => client.E().GetAwaiter().GetResult());
        Assert.Equal("ConstructorInjectedFilterAttributeFilterFactoryTestFilterAttributeHogemoge", ex.Status.Detail);
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

public class ServiceFilterForMethodTestFilterAttribute : MagicOnionFilterAttribute
{
    public ServiceFilterForMethodTestFilterAttribute()
    {
    }

    public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var prevDetail = context.CallContext?.Status.Detail ?? string.Empty;
        context.CallContext.Status = new Grpc.Core.Status(StatusCode.Unknown, prevDetail + nameof(ServiceFilterForMethodTestFilterAttribute));
        return next(context);
    }
}

public interface IFilterConstructorInjectionTester : IService<IFilterConstructorInjectionTester>
{
    UnaryResult<int> A();
    UnaryResult<int> B();
    UnaryResult<int> C();
    UnaryResult<int> D();
    UnaryResult<int> E();
}

public class FilterFactoryTestFilterAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>
{
    public int Order { get; set; }

    public string Name { get; }

    public FilterFactoryTestFilterAttribute(string name)
    {
        Name = name;
    }

    public MagicOnionFilterAttribute CreateInstance(IServiceProvider serviceProvider)
    {
        return new FilterImpl(Name) { Order = Order };
    }

    public class FilterImpl : MagicOnionFilterAttribute
    {
        private readonly string name;

        public FilterImpl(string name)
        {
            this.name = name;
        }

        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            var prevDetail = context.CallContext?.Status.Detail ?? string.Empty;
            context.CallContext.Status = new Grpc.Core.Status(StatusCode.Unknown, prevDetail + nameof(FilterFactoryTestFilterAttribute) + name);
            return next(context);
        }
    }
}

[FromTypeFilter(typeof(ConstructorInjectedFilterAttribute))]
public class FilterConstructorInjectionTester : ServiceBase<IFilterConstructorInjectionTester>, IFilterConstructorInjectionTester
{
    public UnaryResult<int> A()
    {
        return UnaryResult.FromResult(0);
    }

    [FromTypeFilter(typeof(ConstructorInjectedFilter2Attribute))]
    public UnaryResult<int> B()
    {
        return UnaryResult.FromResult(0);
    }

    [FromTypeFilter(typeof(ConstructorInjectedFilter3Attribute), Arguments = new object[] { "foo", 987654 })]
    public UnaryResult<int> C()
    {
        return UnaryResult.FromResult(0);
    }

    [FromServiceFilter(typeof(ServiceFilterForMethodTestFilterAttribute))]
    public UnaryResult<int> D()
    {
        return UnaryResult.FromResult(0);
    }

    [FilterFactoryTestFilter("Hogemoge")]
    public UnaryResult<int> E()
    {
        return UnaryResult.FromResult(0);
    }
}
