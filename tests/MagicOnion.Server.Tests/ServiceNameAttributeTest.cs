using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests
{
    public class ServiceNameAttributeTest
    {
        [Fact]
        public void MapMagicOnionService_SameShortName_DifferentNamespaces_WithAttribute()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMagicOnion();
            var app = builder.Build();
            var routeBuilder = new TestEndpointRouteBuilder(app.Services);

            // Act — register both services with the same short name but different namespaces
            routeBuilder.MapMagicOnionService([
                typeof(ServiceNameAttrAreaA.ProfileAccessService),
                typeof(ServiceNameAttrAreaB.ProfileAccessService)
            ]);

            // Assert — endpoints should use the custom [ServiceName] values, not the short name
            var endpoints = routeBuilder.DataSources.First().Endpoints;
            var displayNames = endpoints.Select(x => x.DisplayName!).ToArray();

            Assert.Contains(displayNames, x => x == "gRPC - /ServiceNameAttrAreaA.IProfileAccess/GetProfileAsync");
            Assert.Contains(displayNames, x => x == "gRPC - /ServiceNameAttrAreaB.IProfileAccess/GetProfileAsync");
        }

        [Fact]
        public void MapMagicOnionService_Hub_SameShortName_DifferentNamespaces_WithAttribute()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMagicOnion();
            var app = builder.Build();
            var routeBuilder = new TestEndpointRouteBuilder(app.Services);

            // Act — register both hubs with the same short name but different namespaces
            routeBuilder.MapMagicOnionService([
                typeof(ServiceNameAttrAreaA.ChatHubService),
                typeof(ServiceNameAttrAreaB.ChatHubService)
            ]);

            // Assert — endpoints should use the custom [ServiceName] values, not the short name
            var endpoints = routeBuilder.DataSources.First().Endpoints;
            var displayNames = endpoints.Select(x => x.DisplayName!).ToArray();

            Assert.Contains(displayNames, x => x == "gRPC - /ServiceNameAttrAreaA.IChatHub/Connect");
            Assert.Contains(displayNames, x => x == "gRPC - /ServiceNameAttrAreaB.IChatHub/Connect");
        }

        [Fact]
        public void MapMagicOnionService_WithoutAttribute_UsesShortName()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMagicOnion();
            var app = builder.Build();
            var routeBuilder = new TestEndpointRouteBuilder(app.Services);

            // Act
            routeBuilder.MapMagicOnionService([typeof(ServiceNameAttrAreaC.FooService)]);

            // Assert — should use short type name (backward compatible)
            var endpoints = routeBuilder.DataSources.First().Endpoints;
            Assert.Contains(endpoints, x => x.DisplayName == "gRPC - /IFooService/DoAsync");
        }

        class TestEndpointRouteBuilder(IServiceProvider serviceProvider) : IEndpointRouteBuilder
        {
            public IList<EndpointDataSource> DataSourcesList { get; } = new List<EndpointDataSource>();

            public IApplicationBuilder CreateApplicationBuilder()
                => new ApplicationBuilder(ServiceProvider);
            public IServiceProvider ServiceProvider
                => serviceProvider;
            public ICollection<EndpointDataSource> DataSources
                => DataSourcesList;
        }
    }
}

namespace ServiceNameAttrAreaA
{
    [MagicOnion.ServiceName("ServiceNameAttrAreaA.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }

    public class ProfileAccessService : MagicOnion.Server.ServiceBase<IProfileAccess>, IProfileAccess
    {
        public MagicOnion.UnaryResult<string> GetProfileAsync() => MagicOnion.UnaryResult.FromResult("AreaA");
    }

    [MagicOnion.ServiceName("ServiceNameAttrAreaA.IChatHub")]
    public interface IChatHub : MagicOnion.IStreamingHub<IChatHub, IChatHubReceiver>
    {
        ValueTask SendAsync(string message);
    }
    public interface IChatHubReceiver
    {
        void OnReceive(string message);
    }

    public class ChatHubService : MagicOnion.Server.Hubs.StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        public ValueTask SendAsync(string message) => ValueTask.CompletedTask;
    }
}

namespace ServiceNameAttrAreaB
{
    [MagicOnion.ServiceName("ServiceNameAttrAreaB.IProfileAccess")]
    public interface IProfileAccess : MagicOnion.IService<IProfileAccess>
    {
        MagicOnion.UnaryResult<string> GetProfileAsync();
    }

    public class ProfileAccessService : MagicOnion.Server.ServiceBase<IProfileAccess>, IProfileAccess
    {
        public MagicOnion.UnaryResult<string> GetProfileAsync() => MagicOnion.UnaryResult.FromResult("AreaB");
    }

    [MagicOnion.ServiceName("ServiceNameAttrAreaB.IChatHub")]
    public interface IChatHub : MagicOnion.IStreamingHub<IChatHub, IChatHubReceiver>
    {
        ValueTask SendAsync(string message);
    }
    public interface IChatHubReceiver
    {
        void OnReceive(string message);
    }

    public class ChatHubService : MagicOnion.Server.Hubs.StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        public ValueTask SendAsync(string message) => ValueTask.CompletedTask;
    }
}

namespace ServiceNameAttrAreaC
{
    // No [ServiceName] attribute — should use short name
    public interface IFooService : MagicOnion.IService<IFooService>
    {
        MagicOnion.UnaryResult<string> DoAsync();
    }

    public class FooService : MagicOnion.Server.ServiceBase<IFooService>, IFooService
    {
        public MagicOnion.UnaryResult<string> DoAsync() => MagicOnion.UnaryResult.FromResult("Foo");
    }
}
