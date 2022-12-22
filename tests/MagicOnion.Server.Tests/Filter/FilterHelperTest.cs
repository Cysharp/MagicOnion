using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests.Filter;

public class FilterHelperTest
{
    [Fact]
    public void GetFilters_Service_Global()
    {
        // Arrange
        var globalFilters = new List<MagicOnionServiceFilterDescriptor>();
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("Global.Instance")));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(typeof(TestFilterAttribute)));

        var methodInfo = new TestService().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(4);
        filters[0].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Global.Instance");
        filters[1].Filter.Should().BeOfType<MagicOnionFilterDescriptor<IMagicOnionServiceFilter>.MagicOnionFilterFromTypeFactory>().Which.Type.Should().Be(typeof(TestFilterAttribute));
        filters[2].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
        filters[3].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Method");
    }

    [Fact]
    public void GetFilters_Service_Class()
    {
        // Arrange
        var methodInfo = new TestService().NoFilteredMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<MagicOnionServiceFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
    }

    [Fact]
    public void GetFilters_Service_Method()
    {
        // Arrange
        var methodInfo = new TestService().Method;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<MagicOnionServiceFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
        filters[1].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Method");
    }

    [Fact]
    public void GetFilters_Service_IgnoreUnknownFilter()
    {
        // Arrange
        var methodInfo = new TestService().UnknownFilterMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<MagicOnionServiceFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
    }

    [Fact]
    public void GetFilters_Service_Order()
    {
        // Arrange
        var globalFilters = new List<MagicOnionServiceFilterDescriptor>();
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("Unordered.1")));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("1"), 1));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("123"), 123));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("-123"), -123));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("Unordered.2")));

        var methodInfo = new TestServiceOrder().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(5 + 4 + 4);
        filters.Select(x => x.Filter).OfType<TestFilterAttribute>().Select(x => x.Name).Should().Equal(new[]
        {
            "-256", "-123", "-64", "1", "64", "123", "256",
            "Unordered.1", "Unordered.2", "Unordered.3", "Unordered.4", "Unordered.5", "Unordered.6"
        });
    }

    [Fact]
    public void GetFilters_Service_LegacyCompatAttributeFactory()
    {
        // Arrange
        var globalFilters = new List<MagicOnionServiceFilterDescriptor>();
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new LegacyFilterFactory()));
        var methodInfo = new TestService().NoFilteredMethod;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<LegacyFilterFactory>();
        filters[1].Filter.Should().BeOfType<TestFilterAttribute>();
    }

    [Fact]
    public void GetFilters_Service_LegacyCompatAttributeFactoryAttribute()
    {
        // Arrange
        var methodInfo = new TestService().LegacyFilterFactoryAttributeAttachedMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<MagicOnionServiceFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<LegacyFilterFactoryAttribute>();
        filters[1].Filter.Should().BeOfType<TestFilterAttribute>();
    }

    [TestFilter("Attribute.Class")]
    class TestService
    {
        [TestFilter("Attribute.Method")]
        public UnaryResult<int> Method() => default;

        public UnaryResult<int> NoFilteredMethod() => default;

        [MetadataOnlyFilter]
        public UnaryResult<int> UnknownFilterMethod() => default;

        public UnaryResult<int> LegacyFilterFactoryAttributeAttachedMethod() => default;
    }

    [TestFilter("Unordered.3")]
    [TestFilter("64", Order = 64)]
    [TestFilter("-256", Order = -256)]
    [TestFilter("Unordered.4")]
    class TestServiceOrder
    {
        [TestFilter("Unordered.5")]
        [TestFilter("-64", Order = -64)]
        [TestFilter("256", Order = 256)]
        [TestFilter("Unordered.6")]
        public UnaryResult<int> Method() => default;
    }

    class MetadataOnlyFilterAttribute : Attribute, IMagicOnionFilterMetadata
    { }

    class TestFilterAttribute : MagicOnionFilterAttribute
    {
        public string Name { get; }

        public TestFilterAttribute() : this("Unnamed")
        {
        }

        public TestFilterAttribute(string name)
        {
            Name = name;
        }

        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => default;
    }

    class LegacyFilterFactory : IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public MagicOnionFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new FilterImpl();
        }

        public int Order { get; }

        public class FilterImpl : MagicOnionFilterAttribute
        {
            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                return next(context);
            }
        }
    }
    
    class LegacyFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<MagicOnionFilterAttribute>, IMagicOnionOrderedFilter
    {
        public MagicOnionFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new FilterImpl();
        }

        public int Order { get; }

        public class FilterImpl : MagicOnionFilterAttribute
        {
            public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            {
                return next(context);
            }
        }
    }

    [Fact]
    public void GetFilters_StreamingHub_Global()
    {
        // Arrange
        var globalFilters = new List<StreamingHubFilterDescriptor>();
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("Global.Instance")));
        globalFilters.Add(new StreamingHubFilterDescriptor(typeof(TestHubFilterAttribute)));

        var methodInfo = new TestHub().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(4);
        filters[0].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Global.Instance");
        filters[1].Filter.Should().BeOfType<MagicOnionFilterDescriptor<IStreamingHubFilter>.MagicOnionFilterFromTypeFactory>().Which.Type.Should().Be(typeof(TestHubFilterAttribute));
        filters[2].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
        filters[3].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Method");
    }

    [Fact]
    public void GetFilters_StreamingHub_Class()
    {
        // Arrange
        var methodInfo = new TestHub().NoFilteredMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<StreamingHubFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
    }

    [Fact]
    public void GetFilters_StreamingHub_Method()
    {
        // Arrange
        var methodInfo = new TestHub().Method;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<StreamingHubFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
        filters[1].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Method");
    }

    [Fact]
    public void GetFilters_StreamingHub_IgnoreUnknownFilter()
    {
        // Arrange
        var methodInfo = new TestHub().UnknownFilterMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<StreamingHubFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
    }

    [Fact]
    public void GetFilters_StreamingHub_Dual()
    {
        // Arrange
        var methodInfo = new TestHubDual().NoFilteredMethod;

        // Act
        var serviceFilters = FilterHelper.GetFilters(Array.Empty<MagicOnionServiceFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);
        var streamingHubFilters = FilterHelper.GetFilters(Array.Empty<StreamingHubFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        serviceFilters.Should().HaveCount(1);
        serviceFilters[0].Filter.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
        streamingHubFilters.Should().HaveCount(1);
        streamingHubFilters[0].Filter.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Attribute.Class");
    }

    [Fact]
    public void GetFilters_StreamingHub_Order()
    {
        // Arrange
        var globalFilters = new List<StreamingHubFilterDescriptor>();
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("Unordered.1")));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("1"), 1));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("123"), 123));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("-123"), -123));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("Unordered.2")));

        var methodInfo = new TestHubOrder().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(5 + 4 + 4);
        filters.Select(x => x.Filter).OfType<TestHubFilterAttribute>().Select(x => x.Name).Should().Equal(new[]
        {
            "-256", "-123", "-64", "1", "64", "123", "256",
            "Unordered.1", "Unordered.2", "Unordered.3", "Unordered.4", "Unordered.5", "Unordered.6"
        });
    }

    [Fact]
    public void GetFilters_StreamingHub_LegacyCompatAttributeFactory()
    {
        // Arrange
        var globalFilters = new List<StreamingHubFilterDescriptor>();
        globalFilters.Add(new StreamingHubFilterDescriptor(new LegacyHubFilterFactory()));
        var methodInfo = new TestHub().NoFilteredMethod;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<LegacyHubFilterFactory>();
        filters[1].Filter.Should().BeOfType<TestHubFilterAttribute>();
    }
    
    [Fact]
    public void GetFilters_StreamingHub_LegacyCompatAttributeFactoryAttribute()
    {
        // Arrange
        var methodInfo = new TestHub().LegacyFilterFactoryAttributeAttachedMethod;

        // Act
        var filters = FilterHelper.GetFilters(Array.Empty<StreamingHubFilterDescriptor>(), methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(2);
        filters[0].Filter.Should().BeOfType<LegacyHubFilterFactoryAttribute>();
        filters[1].Filter.Should().BeOfType<TestHubFilterAttribute>();
    }

    [TestHubFilter("Attribute.Class")]
    class TestHub
    {
        [TestHubFilter("Attribute.Method")]
        public Task<int> Method() => default;

        public Task<int> NoFilteredMethod() => default;

        [MetadataOnlyHubFilter]
        public Task<int> UnknownFilterMethod() => default;

        [LegacyHubFilterFactory]
        public Task<int> LegacyFilterFactoryAttributeAttachedMethod() => default;
    }

    [TestFilter("Attribute.Class")]
    [TestHubFilter("Attribute.Class")]
    class TestHubDual
    {
        [TestHubFilter("Attribute.Method")]
        public Task<int> Method() => default;

        public Task<int> NoFilteredMethod() => default;
    }

    [TestHubFilter("Unordered.3")]
    [TestHubFilter("64", Order = 64)]
    [TestHubFilter("-256", Order = -256)]
    [TestHubFilter("Unordered.4")]
    class TestHubOrder
    {
        [TestHubFilter("Unordered.5")]
        [TestHubFilter("-64", Order = -64)]
        [TestHubFilter("256", Order = 256)]
        [TestHubFilter("Unordered.6")]
        public Task<int> Method() => default;
    }

    class MetadataOnlyHubFilterAttribute : Attribute, IMagicOnionFilterMetadata
    { }

    class LegacyHubFilterFactory : IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public StreamingHubFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new FilterImpl();
        }

        public int Order { get; }

        public class FilterImpl : StreamingHubFilterAttribute
        {
            public override ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
            {
                return next(context);
            }
        }
    }

    class LegacyHubFilterFactoryAttribute : Attribute, IMagicOnionFilterFactory<StreamingHubFilterAttribute>, IMagicOnionOrderedFilter
    {
        public StreamingHubFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new FilterImpl();
        }

        public int Order { get; }

        public class FilterImpl : StreamingHubFilterAttribute
        {
            public override ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
            {
                return next(context);
            }
        }
    }

    class TestHubFilterAttribute : StreamingHubFilterAttribute
    {
        public string Name { get; }

        public TestHubFilterAttribute() : this("Unnamed")
        {
        }

        public TestHubFilterAttribute(string name)
        {
            Name = name;
        }

        public override ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next) => default;
    }

    [Fact]
    public void CreateOrGetInstance_Service_FromType()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var filterDesc = new MagicOnionServiceFilterDescriptor(typeof(TestFilterAttribute));

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Unnamed");
    }

    [Fact]
    public void CreateOrGetInstance_Service_SingletonInstance()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var filterDesc = new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("Instantiated"));

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().Be(filterDesc.Filter);
        instance.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("Instantiated");
    }

    [Fact]
    public void CreateOrGetInstance_Service_Factory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("ValueFromServiceProvider");
        var serviceProvider = services.BuildServiceProvider();
        var filterDesc = new MagicOnionServiceFilterDescriptor(new TestServiceFilterFactory());

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("ValueFromServiceProvider");
    }

    [Fact]
    public void CreateOrGetInstance_Service_Factory_Legacy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("ValueFromServiceProvider");
        var serviceProvider = services.BuildServiceProvider();
        var filterDesc = new MagicOnionServiceFilterDescriptor(new TestServiceFilterLegacyFactory()); /* IMagicOnionFilterFactory<MagicOnionFilterAttribute> */

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestFilterAttribute>().Which.Name.Should().Be("ValueFromServiceProvider");
    }

    class TestServiceFilterFactory : IMagicOnionFilterFactory<IMagicOnionServiceFilter>
    {
        public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceLocator)
        {
            return new TestFilterAttribute(serviceLocator.GetService<string>());
        }

        public int Order { get; }
    }

    class TestServiceFilterLegacyFactory : IMagicOnionFilterFactory<MagicOnionFilterAttribute>
    {
        public MagicOnionFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new TestFilterAttribute(serviceLocator.GetService<string>());
        }

        public int Order { get; }
    }

    [Fact]
    public void CreateOrGetInstance_StreamingHub_FromType()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var filterDesc = new StreamingHubFilterDescriptor(typeof(TestHubFilterAttribute));

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Unnamed");
    }

    [Fact]
    public void CreateOrGetInstance_StreamingHub_SingletonInstance()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var filterDesc = new StreamingHubFilterDescriptor(new TestHubFilterAttribute("Instantiated"));

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().Be(filterDesc.Filter);
        instance.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("Instantiated");
    }

    [Fact]
    public void CreateOrGetInstance_StreamingHub_Factory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("ValueFromServiceProvider");
        var serviceProvider = services.BuildServiceProvider();
        var filterDesc = new StreamingHubFilterDescriptor(new TestStreamingHubFilterFactory());

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("ValueFromServiceProvider");
    }

    [Fact]
    public void CreateOrGetInstance_StreamingHub_Factory_Legacy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("ValueFromServiceProvider");
        var serviceProvider = services.BuildServiceProvider();
        var filterDesc = new StreamingHubFilterDescriptor(new TestStreamingHubLegacyFilterFactory()); /* IMagicOnionFilterFactory<StreamingHubFilterAttribute> */

        // Act
        var instance = FilterHelper.CreateOrGetInstance(serviceProvider, filterDesc);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestHubFilterAttribute>().Which.Name.Should().Be("ValueFromServiceProvider");
    }

    class TestStreamingHubFilterFactory : IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        public IStreamingHubFilter CreateInstance(IServiceProvider serviceLocator)
        {
            return new TestHubFilterAttribute(serviceLocator.GetService<string>());
        }

        public int Order { get; }
    }

    class TestStreamingHubLegacyFilterFactory : IMagicOnionFilterFactory<StreamingHubFilterAttribute>
    {
        public StreamingHubFilterAttribute CreateInstance(IServiceProvider serviceLocator)
        {
            return new TestHubFilterAttribute(serviceLocator.GetService<string>());
        }

        public int Order { get; }
    }

    [Fact]
    public async Task WrapMethodBodyWithFilter_Activate_Service()
    {
        // Arrange
        var results = new List<int>();
        var services = new ServiceCollection();
        services.AddSingleton(results);
        var serviceProvider = services.BuildServiceProvider();
        var filters = new[]
        {
            new MagicOnionServiceFilterDescriptor(new ListIntInjectedServiceFilter(1)),
            new MagicOnionServiceFilterDescriptor(new ListIntInjectedServiceFilter(2)),
        };

        // Act
        var body = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (context) =>
        {
            results.Add(0);
            return default;
        });
        await body(default);

        // Assert
        results.Should().Equal(1, 2, 0, 200, 100);
    }

    class ListIntInjectedServiceFilter : IMagicOnionFilterFactory<IMagicOnionServiceFilter>
    {
        readonly int baseValue;

        public ListIntInjectedServiceFilter(int baseValue)
        {
            this.baseValue = baseValue;
        }

        public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceLocator)
        {
            var results = serviceLocator.GetService<List<int>>();
            return new DelegateServiceFilter(async (context, next) =>
            {
                results.Add(1 * baseValue);
                await next(context);
                results.Add(100 * baseValue);
            });
        }

        public int Order { get; }
    }

    [Fact]
    public async Task WrapMethodBodyWithFilter_Activate_StreamingHub()
    {
        // Arrange
        var results = new List<int>();
        var services = new ServiceCollection();
        services.AddSingleton(results);
        var serviceProvider = services.BuildServiceProvider();
        var filters = new[]
        {
            new StreamingHubFilterDescriptor(new ListIntInjectedStreamingHubFilter(1)),
            new StreamingHubFilterDescriptor(new ListIntInjectedStreamingHubFilter(2)),
        };

        // Act
        var body = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (context) =>
        {
            results.Add(0);
            return default;
        });
        await body(default);

        // Assert
        results.Should().Equal(1, 2, 0, 200, 100);
    }

    class ListIntInjectedStreamingHubFilter : IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        readonly int baseValue;

        public ListIntInjectedStreamingHubFilter(int baseValue)
        {
            this.baseValue = baseValue;
        }

        public IStreamingHubFilter CreateInstance(IServiceProvider serviceLocator)
        {
            var results = serviceLocator.GetService<List<int>>();
            return new DelegateHubFilter(async (context, next) =>
            {
                results.Add(1 * baseValue);
                await next(context);
                results.Add(100 * baseValue);
            });
        }

        public int Order { get; }
    }

    [Fact]
    public async Task WrapMethodBodyWithFilter_Surround_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var results = new List<int>();
        var filters = new[]
        {
            new MagicOnionServiceFilterDescriptor(new DelegateServiceFilter(async (context, next) =>
            {
                results.Add(1);
                await next(context);
                results.Add(100);
            })),
            new MagicOnionServiceFilterDescriptor(new DelegateServiceFilter(async (context, next) =>
            {
                results.Add(2);
                await next(context);
                results.Add(200);
            })),
        };

        // Act
        var body = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (context) =>
        {
            results.Add(0);
            return default;
        });
        await body(default);

        // Assert
        results.Should().Equal(1, 2, 0, 200, 100);
    }

    class DelegateServiceFilter : IMagicOnionServiceFilter
    {
        readonly Func<ServiceContext, Func<ServiceContext, ValueTask>, ValueTask> func;

        public DelegateServiceFilter(Func<ServiceContext, Func<ServiceContext, ValueTask>, ValueTask> func)
        {
            this.func = func;
        }

        public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
            => func(context, next);
    }

    [Fact]
    public async Task WrapMethodBodyWithFilter_Surround_StreamingHub()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var results = new List<int>();
        var filters = new[]
        {
            new StreamingHubFilterDescriptor(new DelegateHubFilter(async (context, next) =>
            {
                results.Add(1);
                await next(context);
                results.Add(100);
            })),
            new StreamingHubFilterDescriptor(new DelegateHubFilter(async (context, next) =>
            {
                results.Add(2);
                await next(context);
                results.Add(200);
            })),
        };

        // Act
        var body = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (context) =>
        {
            results.Add(0);
            return default;
        });
        await body(default);

        // Assert
        results.Should().Equal(1, 2, 0, 200, 100);
    }

    class DelegateHubFilter : IStreamingHubFilter
    {
        readonly Func<StreamingHubContext, Func<StreamingHubContext, ValueTask>, ValueTask> func;

        public DelegateHubFilter(Func<StreamingHubContext, Func<StreamingHubContext, ValueTask>, ValueTask> func)
        {
            this.func = func;
        }

        public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
            => func(context, next);
    }
}
