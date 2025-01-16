using MagicOnion.Server.Filters;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Tests.Filter;

public class MagicOnionFilterDescriptorExtensionsTest
{
    [Fact]
    public void Add_Service_PreserveOrder()
    {
        // Arrange
        var filters = new List<MagicOnionServiceFilterDescriptor>();

        // Act
        filters.Add(new ServiceFilter() { Order = 123 });

        // Assert
        Assert.IsType<ServiceFilter>(filters[0].Filter);
        Assert.Equal(123, filters[0].Order);
    }

    [Fact]
    public void Add_StreamingHub_PreserveOrder()
    {
        // Arrange
        var filters = new List<StreamingHubFilterDescriptor>();

        // Act
        filters.Add(new StreamingHubFilter() { Order = 123 });

        // Assert
        Assert.IsType<StreamingHubFilter>(filters[0].Filter);
        Assert.Equal(123, filters[0].Order);
    }

    [Fact]
    public void Add_Service_Factory_Instance()
    {
        // Arrange
        var filters = new List<MagicOnionServiceFilterDescriptor>();

        // Act
        filters.Add(new ServiceFilterFactory());

        // Assert
        Assert.IsType<ServiceFilterFactory>(filters[0].Filter);
    }

    [Fact]
    public void Add_Service_Factory_Type()
    {
        // Arrange
        var filters = new List<MagicOnionServiceFilterDescriptor>();

        // Act
        filters.Add<ServiceFilterFactory>();

        // Assert
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IMagicOnionServiceFilter>.MagicOnionFilterFromTypeFactoryFactory>(filters[0].Filter);
        Assert.Equal(typeof(ServiceFilterFactory), filter.Type);
    }

    [Fact]
    public void Add_StreamingHub_Factory_Instance()
    {
        // Arrange
        var filters = new List<StreamingHubFilterDescriptor>();

        // Act
        filters.Add(new StreamingHubFilterFactory());

        // Assert
        Assert.IsType<StreamingHubFilterFactory>(filters[0].Filter);
    }

    [Fact]
    public void Add_StreamingHub_Factory_Type()
    {
        // Arrange
        var filters = new List<StreamingHubFilterDescriptor>();

        // Act
        filters.Add<StreamingHubFilterFactory>();

        // Assert
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IStreamingHubFilter>.MagicOnionFilterFromTypeFactoryFactory>(filters[0].Filter); // `MagicOnionFilterFromTypeFactoryFactory` is internal type
        Assert.Equal(typeof(StreamingHubFilterFactory), filter.Type);
    }

    class ServiceFilter : IMagicOnionServiceFilter, IMagicOnionOrderedFilter
    {
        public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => next(context);

        public int Order { get; set; }
    }

    class StreamingHubFilter : IStreamingHubFilter, IMagicOnionOrderedFilter
    {
        public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next) => next(context);

        public int Order { get; set; }
    }

    class ServiceFilterFactory : IMagicOnionFilterFactory<IMagicOnionServiceFilter>
    {
        public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceLocator)
            => new FilterImpl();

        public class FilterImpl : IMagicOnionServiceFilter
        {
            public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
                => next(context);
        }

        public int Order { get; }
    }

    class StreamingHubFilterFactory : IMagicOnionFilterFactory<IStreamingHubFilter>
    {
        public IStreamingHubFilter CreateInstance(IServiceProvider serviceLocator)
            => new FilterImpl();

        public class FilterImpl : IStreamingHubFilter
        {
            public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
                => next(context);
        }

        public int Order { get; }
    }
}
