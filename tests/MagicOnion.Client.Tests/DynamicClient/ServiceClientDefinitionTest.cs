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
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.Unary))!);
        Assert.NotNull(methodInfo);
        Assert.Equal(nameof(IDummyService.Unary), methodInfo.MethodName);
        Assert.Equal(typeof(UnaryResult<int>), methodInfo.MethodReturnType);
        Assert.Equal(typeof(int), methodInfo.ResponseType);
    }
    
    [Fact]
    public void Unary_NonGeneric()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.UnaryNonGeneric))!);
        Assert.NotNull(methodInfo);
        Assert.Equal(nameof(IDummyService.UnaryNonGeneric), methodInfo.MethodName);
        Assert.Equal(typeof(UnaryResult), methodInfo.MethodReturnType);
        Assert.Equal(typeof(Nil), methodInfo.ResponseType);
    }

    [Fact]
    public void ClientStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.ClientStreaming))!);
        Assert.NotNull(methodInfo);
        Assert.Equal(nameof(IDummyService.ClientStreaming), methodInfo.MethodName);
        Assert.Equal(typeof(Task<ClientStreamingResult<int, int>>), methodInfo.MethodReturnType);
    }

    [Fact]
    public void ServerStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.ServerStreaming))!);
        Assert.NotNull(methodInfo);
        Assert.Equal(nameof(IDummyService.ServerStreaming), methodInfo.MethodName);
        Assert.Equal(typeof(Task<ServerStreamingResult<int>>), methodInfo.MethodReturnType);
    }

    [Fact]
    public void DuplexStreaming()
    {
        var methodInfo = ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.DuplexStreaming))!);
        Assert.NotNull(methodInfo);
        Assert.Equal(nameof(IDummyService.DuplexStreaming), methodInfo.MethodName);
        Assert.Equal(typeof(Task<DuplexStreamingResult<int, int>>), methodInfo.MethodReturnType);
    }
    [Fact]
    public void InvalidUnaryTaskOfUnary()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.TaskOfUnary))!);
        });

        Assert.Contains("The return type of an Unary method must be 'UnaryResult' or 'UnaryResult<T>'", ex.Message);
    }

    [Fact]
    public void InvalidNonTaskOfClientStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfClientStreamingResult))!);
        });

        Assert.Contains("The return type of a Streaming method must be 'Task<", ex.Message);
    }

    [Fact]
    public void InvalidNonTaskOfServerStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfServerStreamingResult))!);
        });

        Assert.Contains("The return type of a Streaming method must be 'Task<", ex.Message);
    }

    [Fact]
    public void InvalidNonTaskOfDuplexStreamingResult()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.NonTaskOfDuplexStreamingResult))!);
        });

        Assert.Contains("The return type of a Streaming method must be 'Task<", ex.Message);
    }

    [Fact]
    public void InvalidUnknown()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.Unknown))!);
        });

        Assert.Contains("The method of a service must return 'UnaryResult<T>', 'Task<ClientStreamingResult<TRequest, TResponse>>'", ex.Message);
    }
}
