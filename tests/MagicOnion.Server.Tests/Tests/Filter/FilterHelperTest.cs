using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests.Tests.Filter;

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
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("1"), 1));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("123"), 123));
        globalFilters.Add(new MagicOnionServiceFilterDescriptor(new TestFilterAttribute("-123"), -123));

        var methodInfo = new TestServiceOrder().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(3 + 4);
        filters.Select(x => x.Filter).OfType<TestFilterAttribute>().Select(x => x.Name).Should().Equal(new[]
        {
            "-256", "-123", "-64", "1", "64", "123", "256",
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

    [TestFilter("Attribute.Class")]
    class TestService
    {
        [TestFilter("Attribute.Method")]
        public UnaryResult<int> Method() => default;

        public UnaryResult<int> NoFilteredMethod() => default;

        [MetadataOnlyFilter]
        public UnaryResult<int> UnknownFilterMethod() => default;
    }
    
    [TestFilter("64", Order = 64)]
    [TestFilter("-256", Order = -256)]
    class TestServiceOrder
    {
        [TestFilter("-64", Order = -64)]
        [TestFilter("256", Order = 256)]
        public UnaryResult<int> Method() => default;
    }

    class MetadataOnlyFilterAttribute : Attribute, IMagicOnionFilterMetadata
    {}

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
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("1"), 1));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("123"), 123));
        globalFilters.Add(new StreamingHubFilterDescriptor(new TestHubFilterAttribute("-123"), -123));

        var methodInfo = new TestHubOrder().Method;

        // Act
        var filters = FilterHelper.GetFilters(globalFilters, methodInfo.Target!.GetType(), methodInfo.Method);

        // Assert
        filters.Should().HaveCount(3 + 4);
        filters.Select(x => x.Filter).OfType<TestHubFilterAttribute>().Select(x => x.Name).Should().Equal(new[]
        {
            "-256", "-123", "-64", "1", "64", "123", "256",
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

    [TestHubFilter("Attribute.Class")]
    class TestHub
    {
        [TestHubFilter("Attribute.Method")]
        public Task<int> Method() => default;

        public Task<int> NoFilteredMethod() => default;

        [MetadataOnlyHubFilter]
        public Task<int> UnknownFilterMethod() => default;
    }

    [TestFilter("Attribute.Class")]
    [TestHubFilter("Attribute.Class")]
    class TestHubDual
    {
        [TestHubFilter("Attribute.Method")]
        public Task<int> Method() => default;

        public Task<int> NoFilteredMethod() => default;
    }
    
    [TestHubFilter("64", Order = 64)]
    [TestHubFilter("-256", Order = -256)]
    class TestHubOrder
    {
        [TestHubFilter("-64", Order = -64)]
        [TestHubFilter("256", Order = 256)]
        public Task<int> Method() => default;
    }

    class MetadataOnlyHubFilterAttribute : Attribute, IMagicOnionFilterMetadata
    {}

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
        var instance = FilterHelper.CreateOrGetInstance<IMagicOnionServiceFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IMagicOnionServiceFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IMagicOnionServiceFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IMagicOnionServiceFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IStreamingHubFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IStreamingHubFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IStreamingHubFilter>(serviceProvider, filterDesc);

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
        var instance = FilterHelper.CreateOrGetInstance<IStreamingHubFilter>(serviceProvider, filterDesc);

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
}