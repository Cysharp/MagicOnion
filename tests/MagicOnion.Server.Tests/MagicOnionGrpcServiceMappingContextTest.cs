using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.Tests;

public class MagicOnionGrpcServiceMappingContextTest
{
    [Fact]
    public void MapMagicOnionService_ValidServiceType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();

        // Act
        var ex = Record.Exception(() => app.MapMagicOnionService([typeof(GreeterService), typeof(GreeterHub)]));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void MapMagicOnionService_InvalidType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();

        // Act
        var ex = Record.Exception(() => app.MapMagicOnionService([typeof(object)]));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void MapMagicOnionService_Service()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();
        var routeBuilder = new TestEndpointRouteBuilder(app.Services);

        // Act
        routeBuilder.MapMagicOnionService([typeof(GreeterService)]);

        // Assert
        Assert.Equal(4, routeBuilder.DataSources.First().Endpoints.Count); // HelloAsync + GoodbyeAsync + unimplemented.method + unimplemented.service
        Assert.Equal($"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.HelloAsync)}", routeBuilder.DataSources.First().Endpoints[0].DisplayName);
        Assert.Equal($"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.GoodbyeAsync)}", routeBuilder.DataSources.First().Endpoints[1].DisplayName);
    }

    [Fact]
    public void MapMagicOnionService_Hub()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();
        var routeBuilder = new TestEndpointRouteBuilder(app.Services);

        // Act
        routeBuilder.MapMagicOnionService([typeof(GreeterHub)]);

        // Assert
        Assert.Equal(3, routeBuilder.DataSources.First().Endpoints.Count); // Connect + unimplemented.method + unimplemented.service
        Assert.Equal($"gRPC - /{nameof(IGreeterHub)}/Connect", routeBuilder.DataSources.First().Endpoints[0].DisplayName);
    }

    [Fact]
    public void MapMagicOnionService_Metadata()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();
        var routeBuilder = new TestEndpointRouteBuilder(app.Services);

        // Act
        routeBuilder.MapMagicOnionService([typeof(GreeterService)]).RequireAuthorization();

        // Assert
        var endpoints = routeBuilder.DataSources.First().Endpoints;
        var authAttrHelloAsync = endpoints.First(x => x.DisplayName == $"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.HelloAsync)}").Metadata.FirstOrDefault(x => x is AuthorizeAttribute);
        var authAttrGoodbyeAsync = endpoints.First(x => x.DisplayName == $"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.GoodbyeAsync)}").Metadata.FirstOrDefault(x => x is AuthorizeAttribute);
        var allowAnonymousAttrGoodbyeAsync = endpoints.First(x => x.DisplayName == $"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.GoodbyeAsync)}").Metadata.FirstOrDefault(x => x is AllowAnonymousAttribute);
        Assert.NotNull(authAttrHelloAsync);
        Assert.NotNull(authAttrGoodbyeAsync);
        Assert.NotNull(allowAnonymousAttrGoodbyeAsync);
    }


    [Fact]
    public void MapMagicOnionService_MultipleTimes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMagicOnion();
        var app = builder.Build();
        var routeBuilder = new TestEndpointRouteBuilder(app.Services);

        // Act
        routeBuilder.MapMagicOnionService([typeof(GreeterService)]).Add(x => x.Metadata.Add("#1"));
        routeBuilder.MapMagicOnionService([typeof(GreeterHub)]).Add(x => x.Metadata.Add("#2"));

        // Assert
        Assert.Equal(6, routeBuilder.DataSources.First().Endpoints.Count); // IGreeterService.HelloAsync/GoodbyeAsync + unimplemented.service + IGreeterService.unimplemented method + IGreeterHub.Connect + IGreeterHub.unimplemented method
        Assert.Contains("#1", routeBuilder.DataSources.First().Endpoints.First(x => x.DisplayName == $"gRPC - /{nameof(IGreeterService)}/{nameof(IGreeterService.HelloAsync)}").Metadata);
        Assert.Contains("#2", routeBuilder.DataSources.First().Endpoints.First(x => x.DisplayName == $"gRPC - /{nameof(IGreeterHub)}/Connect").Metadata);
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

    public interface IGreeterService : IService<IGreeterService>
    {
        UnaryResult<string> HelloAsync(string name, int age);
        UnaryResult<string> GoodbyeAsync(string name, int age);
    }

    public interface IGreeterHub : IStreamingHub<IGreeterHub, IGreeterHubReceiver>
    {
        ValueTask<string> HelloAsync(string name, int age);
        ValueTask<string> GoodbyeAsync(string name, int age);
    }
    public interface IGreeterHubReceiver;

    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
    {
        public UnaryResult<string> HelloAsync(string name, int age) => UnaryResult.FromResult($"Hello {name} ({age})!");
        [AllowAnonymous]
        public UnaryResult<string> GoodbyeAsync(string name, int age) => UnaryResult.FromResult($"Goodbye {name} ({age})!");
    }

    public class GreeterHub : StreamingHubBase<IGreeterHub, IGreeterHubReceiver>, IGreeterHub
    {
        public ValueTask<string> HelloAsync(string name, int age) => ValueTask.FromResult($"Hello {name} ({age})!");
        public ValueTask<string> GoodbyeAsync(string name, int age) => ValueTask.FromResult($"Goodbye {name} ({age})!");
    }
}
