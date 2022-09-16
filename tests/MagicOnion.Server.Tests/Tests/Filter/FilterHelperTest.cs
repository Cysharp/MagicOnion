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


    [TestFilter("Attribute.Class")]
    class TestService
    {
        [TestFilter("Attribute.Method")]
        public UnaryResult<int> Method() => default;

        public UnaryResult<int> NoFilteredMethod() => default;

        [MetadataOnlyFilter]
        public UnaryResult<int> UnknownFilterMethod() => default;
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

    class MetadataOnlyHubFilterAttribute : Attribute, IMagicOnionFilterMetadata
    {}

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

    class TestServiceFilterFactory : IMagicOnionFilterFactory<IMagicOnionServiceFilter>
    {
        public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceLocator)
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

    class TestStreamingHubFilterFactory : IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        public IStreamingHubFilter CreateInstance(IServiceProvider serviceLocator)
        {
            return new TestHubFilterAttribute(serviceLocator.GetService<string>());
        }

        public int Order { get; }
    }
}