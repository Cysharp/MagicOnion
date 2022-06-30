using MagicOnion.Client.DynamicClient;

namespace MagicOnion.Client.Tests.DynamicClient;

public class ServiceClientDefinitionTest
{
    public interface IDummyService
    {
        Task<UnaryResult<int>> TaskOfUnary();
        ClientStreamingResult<int, int> NonTaskOfClientStreamingResult();
        ServerStreamingResult<int> NonTaskOfServerStreamingResult();
        DuplexStreamingResult<int, int> NonTaskOfDuplexStreamingResult();
        void Unknown();
    }

    [Fact]
    public void InvalidUnaryTaskOfUnary()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceClientDefinition.MagicOnionServiceMethodInfo.Create(typeof(IDummyService), typeof(IDummyService).GetMethod(nameof(IDummyService.TaskOfUnary)));
        });

        ex.Message.Should().Contain("The return type of an Unary method must be 'UnaryResult<T>'");
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
