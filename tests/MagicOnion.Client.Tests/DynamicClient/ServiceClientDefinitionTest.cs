using MagicOnion.Client.DynamicClient;

namespace MagicOnion.Client.Tests.DynamicClient;

public class ServiceClientDefinitionTest
{
    public interface IDummyService
    {
        UnaryResult<int> Unary();
        UnaryResult UnaryNonGeneric();
        Task<ClientStreamingResult<int, int>> ClientStreaming();
        Task<ServerStreamingResult<int>> ServerStreaming();
        Task<DuplexStreamingResult<int, int>> DuplexStreaming();

        Task<UnaryResult<int>> TaskOfUnary();
        ClientStreamingResult<int, int> NonTaskOfClientStreamingResult();
        ServerStreamingResult<int> NonTaskOfServerStreamingResult();
        DuplexStreamingResult<int, int> NonTaskOfDuplexStreamingResult();
        void Unknown();
    }

    [Fact]
    public void Unary()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.Unary)));
        methodInfo.Should().NotBeNull();
        methodInfo.MethodName.Should().Be(nameof(IDummyService.Unary));
        methodInfo.MethodReturnType.Should().Be<UnaryResult<int>>();
        methodInfo.ResponseType.Should().Be<int>();
    }
    
    [Fact]
    public void Unary_NonGeneric()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.UnaryNonGeneric)));
        methodInfo.Should().NotBeNull();
        methodInfo.MethodName.Should().Be(nameof(IDummyService.UnaryNonGeneric));
        methodInfo.MethodReturnType.Should().Be<UnaryResult>();
        methodInfo.ResponseType.Should().Be<Nil>();
    }

    [Fact]
    public void ClientStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.ClientStreaming)));
        methodInfo.Should().NotBeNull();
        methodInfo.MethodName.Should().Be(nameof(IDummyService.ClientStreaming));
        methodInfo.MethodReturnType.Should().Be<Task<ClientStreamingResult<int, int>>>();
    }

    [Fact]
    public void ServerStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.ServerStreaming)));
        methodInfo.Should().NotBeNull();
        methodInfo.MethodName.Should().Be(nameof(IDummyService.ServerStreaming));
        methodInfo.MethodReturnType.Should().Be<Task<ServerStreamingResult<int>>>();
    }

    [Fact]
    public void DuplexStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.DuplexStreaming)));
        methodInfo.Should().NotBeNull();
        methodInfo.MethodName.Should().Be(nameof(IDummyService.DuplexStreaming));
        methodInfo.MethodReturnType.Should().Be<Task<DuplexStreamingResult<int, int>>>();
    }
    [Fact]
    public void InvalidUnaryTaskOfUnary()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.TaskOfUnary)));
        });

        ex.Message.Should().Contain("The return type of an Unary method must be 'UnaryResult' or 'UnaryResult<T>'");
    }

    [Fact]
    public void InvalidNonTaskOfClientStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfClientStreamingResult)));
        });

        ex.Message.Should().Contain("The return type of a Streaming method must be 'Task<");
    }

    [Fact]
    public void InvalidNonTaskOfServerStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfServerStreamingResult)));
        });

        ex.Message.Should().Contain("The return type of a Streaming method must be 'Task<");
    }

    [Fact]
    public void InvalidNonTaskOfDuplexStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfDuplexStreamingResult)));
        });

        ex.Message.Should().Contain("The return type of a Streaming method must be 'Task<");
    }

    [Fact]
    public void InvalidUnknown()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.Unknown)));
        });

        ex.Message.Should().Contain("The method of a service must return 'UnaryResult<T>', 'Task<ClientStreamingResult<TRequest, TResponse>>'");
    }
}
