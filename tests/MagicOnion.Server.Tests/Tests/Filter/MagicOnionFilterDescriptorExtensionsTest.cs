using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests.Tests.Filter;

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
        filters[0].Filter.Should().BeOfType<ServiceFilter>();
        filters[0].Order.Should().Be(123);
    }

    [Fact]
    public void Add_StreamingHub_PreserveOrder()
    {
        // Arrange
        var filters = new List<StreamingHubFilterDescriptor>();

        // Act
        filters.Add(new StreamingHubFilter() { Order = 123 });

        // Assert
        filters[0].Filter.Should().BeOfType<StreamingHubFilter>();
        filters[0].Order.Should().Be(123);
    }

    [Fact]
    public void Add_Service_Factory_Instance()
    {
        // Arrange
        var filters = new List<MagicOnionServiceFilterDescriptor>();

        // Act
        filters.Add(new ServiceFilterFactory());

        // Assert
        filters[0].Filter.Should().BeOfType<ServiceFilterFactory>();
    }

    //[Fact]
    //public void Add_Service_Factory_Type()
    //{
    //    // Arrange
    //    var filters = new List<MagicOnionServiceFilterDescriptor>();

    //    // Act
    //    filters.Add<ServiceFilterFactory>();

    //    // Assert
    //    filters[0].Filter.Should()
    //        .BeOfType<MagicOnionFilterDescriptor<IMagicOnionServiceFilter>.MagicOnionFilterFromTypeFactory>() // `MagicOnionFilterFromTypeFactory` is internal type
    //        .Which.Type.Should().BeOfType<ServiceFilterFactory>();
    //}

    [Fact]
    public void Add_StreamingHub_Factory_Instance()
    {
        // Arrange
        var filters = new List<StreamingHubFilterDescriptor>();

        // Act
        filters.Add(new StreamingHubFilterFactory());

        // Assert
        filters[0].Filter.Should().BeOfType<StreamingHubFilterFactory>();
    }

    //[Fact]
    //public void Add_StreamingHub_Factory_Type()
    //{
    //    // Arrange
    //    var filters = new List<StreamingHubFilterDescriptor>();

    //    // Act
    //    filters.Add<StreamingHubFilterFactory>();

    //    // Assert
    //    filters[0].Filter.Should().BeOfType<MagicOnionFilterDescriptor<IStreamingHubFilter>.MagicOnionFilterFromTypeFactory>() // `MagicOnionFilterFromTypeFactory` is internal type
    //        .Which.Type.Should().BeOfType<StreamingHubFilterFactory>();
    //}

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