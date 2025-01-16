using MagicOnion.Server.Filters;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests.Filter;

public class MagicOnionFilterDescriptorTest
{
    [Fact]
    public void Order_Service_Default()
    {
        // Arrange
        var desc = new MagicOnionServiceFilterDescriptor(new ServiceFilterUnordered());

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(int.MaxValue, order);
    }

    [Fact]
    public void Order_StreamingHub_Default()
    {
        // Arrange
        var desc = new StreamingHubFilterDescriptor(new StreamingHubFilterUnordered());

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(int.MaxValue, order);
    }

    [Fact]
    public void Order_Service_Override()
    {
        // Arrange
        var desc = new MagicOnionServiceFilterDescriptor(new ServiceFilter() { Order = 123 }, order: 256);

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(256, order);
    }

    [Fact]
    public void Order_StreamingHub_Override()
    {
        // Arrange
        var desc = new StreamingHubFilterDescriptor(new StreamingHubFilter() { Order = 123 }, order: 256);

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(256, order);
    }

    [Fact]
    public void Order_Service_PreserveOrder()
    {
        // Arrange
        var desc = new MagicOnionServiceFilterDescriptor(new ServiceFilter() { Order = 123 });

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(123, order);
    }

    [Fact]
    public void Order_StreamingHub_PreserveOrder()
    {
        // Arrange
        var desc = new StreamingHubFilterDescriptor(new StreamingHubFilter() { Order = 123 });

        // Act
        var order = desc.Order;

        // Assert
        Assert.Equal(123, order);
    }

    [Fact]
    public void CreateInstanceFromType_Service_Filter()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var desc = new MagicOnionServiceFilterDescriptor(typeof(ServiceFilter));

        // Act
        var instancedFilter = ((IMagicOnionFilterFactory<IMagicOnionServiceFilter>)desc.Filter).CreateInstance(serviceProvider);

        // Assert
        Assert.IsType<ServiceFilter>(instancedFilter);
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IMagicOnionServiceFilter>.MagicOnionFilterFromTypeFactory>(desc.Filter);
        Assert.Equal(typeof(ServiceFilter), filter.Type);
    }

    [Fact]
    public void CreateInstanceFromType_Service_FilterFactory()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var desc = new MagicOnionServiceFilterDescriptor(typeof(ServiceFilterFactory));

        // Act
        var instancedFilter = ((IMagicOnionFilterFactory<IMagicOnionServiceFilter>)desc.Filter).CreateInstance(serviceProvider);

        // Assert
        Assert.IsType<ServiceFilter>(instancedFilter);
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IMagicOnionServiceFilter>.MagicOnionFilterFromTypeFactoryFactory>(desc.Filter);
        Assert.Equal(typeof(ServiceFilterFactory), filter.Type);
    }

    [Fact]
    public void CreateInstanceFromType_StreamingHub_Filter()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var desc = new StreamingHubFilterDescriptor(typeof(StreamingHubFilter));

        // Act
        var instancedFilter = ((IMagicOnionFilterFactory<IStreamingHubFilter>)desc.Filter).CreateInstance(serviceProvider);

        // Assert
        Assert.IsType<StreamingHubFilter>(instancedFilter);
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IStreamingHubFilter>.MagicOnionFilterFromTypeFactory>(desc.Filter);
        Assert.Equal(typeof(StreamingHubFilter), filter.Type);
    }

    [Fact]
    public void CreateInstanceFromType_StreamingHub_FilterFactory()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var desc = new StreamingHubFilterDescriptor(typeof(StreamingHubFilterFactory));

        // Act
        var instancedFilter = ((IMagicOnionFilterFactory<IStreamingHubFilter>)desc.Filter).CreateInstance(serviceProvider);

        // Assert
        Assert.IsType<StreamingHubFilter>(instancedFilter);
        var filter = Assert.IsType<MagicOnionFilterDescriptor<IStreamingHubFilter>.MagicOnionFilterFromTypeFactoryFactory>(desc.Filter);
        Assert.Equal(typeof(StreamingHubFilterFactory), filter.Type);
    }

    [Fact]
    public void UnsupportedType_Service()
    {
        // Arrange
        var targetType = typeof(StreamingHubFilter);

        // Act
        var exception = Record.Exception(() => new MagicOnionServiceFilterDescriptor(targetType)); // MagicOnionServiceFilterDescriptor can't accept non-IMagicOnionServiceFilter types.

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void UnsupportedType_StreamingHub()
    {
        // Arrange
        var targetType = typeof(ServiceFilter);

        // Act
        var exception = Record.Exception(() => new StreamingHubFilterDescriptor(targetType)); // StreamingHubFilterDescriptor can't accept non-IStreamingHubFilter types.

        // Assert
        Assert.NotNull(exception);
    }

    class ServiceFilter : IMagicOnionServiceFilter, IMagicOnionOrderedFilter
    {
        public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => next(context);
        public int Order { get; set; }
    }

    class ServiceFilterUnordered : IMagicOnionServiceFilter
    {
        public ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next) => next(context);
    }

    class ServiceFilterFactory : IMagicOnionFilterFactory<IMagicOnionServiceFilter>, IMagicOnionOrderedFilter
    {
        public IMagicOnionServiceFilter CreateInstance(IServiceProvider serviceLocator) => new ServiceFilter();
        public int Order { get; set; }
    }

    class StreamingHubFilter : IStreamingHubFilter, IMagicOnionOrderedFilter
    {
        public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next) => next(context);
        public int Order { get; set; }
    }

    class StreamingHubFilterUnordered : IStreamingHubFilter
    {
        public ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next) => next(context);
    }

    class StreamingHubFilterFactory : IMagicOnionFilterFactory<IStreamingHubFilter>, IMagicOnionOrderedFilter
    {
        public IStreamingHubFilter CreateInstance(IServiceProvider serviceLocator) => new StreamingHubFilter();
        public int Order { get; set; }
    }
}
