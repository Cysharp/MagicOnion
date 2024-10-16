using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Builder;
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

    public interface IGreeterService : IService<IGreeterService>;
    public interface IGreeterHub : IStreamingHub<IGreeterHub, IGreeterHubReceiver>;
    public interface IGreeterHubReceiver;
    public class GreeterService : ServiceBase<IGreeterService>, IGreeterService;
    public class GreeterHub : StreamingHubBase<IGreeterHub, IGreeterHubReceiver>, IGreeterHub;
}
