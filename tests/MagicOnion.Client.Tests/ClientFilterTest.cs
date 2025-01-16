namespace MagicOnion.Client.Tests;

public class ClientFilterTest
{
    [Fact]
    public async Task ReturnImmediateValue()
    {
        // Arrange
        var clientFilterMock = Substitute.For<IClientFilter>();
        clientFilterMock
            .SendAsync(default!, default!)
            // NOTE: Mock IClientFilter returns a value immediately. (The filter will not call `next`)
            .ReturnsForAnyArgs(ValueTask.FromResult((ResponseContext)ResponseContext<string>.Create("Response", Status.DefaultSuccess, Metadata.Empty, Metadata.Empty)));
        var callInvokerMock = Substitute.For<CallInvoker>();
        callInvokerMock.AsyncUnaryCall(default(Method<string, string>)!, default, default, default!)
            .Returns(new AsyncUnaryCall<string>(
                Task.FromResult("Response"),
                Task.FromResult(Metadata.Empty),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }));

        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock, new []
        {
            clientFilterMock,
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        Assert.Equal("Response", result);
        callInvokerMock.DidNotReceive();
        clientFilterMock.Received();
    }

    [Fact]
    public async Task RequestHeaders()
    {
        // Arrange
        var requestHeaders = default(Metadata);
        var callInvokerMock = Substitute.For<CallInvoker>();
        callInvokerMock.AsyncUnaryCall(default(Method<string, string>)!, default, default, default!)
            .ReturnsForAnyArgs(x =>
            {
                requestHeaders = x.Arg<CallOptions>().Headers;
                return new AsyncUnaryCall<string>(
                    Task.FromResult("Response"),
                    Task.FromResult(Metadata.Empty),
                    () => Status.DefaultSuccess,
                    () => Metadata.Empty,
                    () => { });
            });
        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock, new []
        {
            new ClientFilterTestRequestHeaders(),
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        Assert.Equal("Response", result);
        Assert.NotNull(requestHeaders);
        Assert.Equal(2, requestHeaders.Count());
        Assert.Contains(requestHeaders, x => x.Key == "x-header-1" && x.Value == "valueA");
        Assert.Contains(requestHeaders, x => x.Key == "x-header-2" && x.Value == "valueB");
    }

    class ClientFilterTestRequestHeaders : IClientFilter
    {
        public ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            context.CallOptions.Headers?.Add("x-header-1", "valueA");
            context.CallOptions.Headers?.Add("x-header-2", "valueB");
            return next(context);
        }
    }

    [Fact]
    public async Task FilterChain()
    {
        // Arrange
        var calledFilters = new List<string>();
        var clientFilterMockFirst = Substitute.For<IClientFilter>();
        clientFilterMockFirst.SendAsync(default!, default!)
            .ReturnsForAnyArgs(x =>
            {
                var context = x.Arg<RequestContext>();
                var next = x.Arg<Func<RequestContext, ValueTask<ResponseContext>>>();
                calledFilters.Add("First");
                return next(context);
            });

        var clientFilterMockSecond = Substitute.For<IClientFilter>();
        clientFilterMockSecond.SendAsync(default!, default!)
            .ReturnsForAnyArgs(x =>
            {
                var context = x.Arg<RequestContext>();
                var next = x.Arg<Func<RequestContext, ValueTask<ResponseContext>>>();
                calledFilters.Add("Second");
                return next(context);
            });

        var callInvokerMock = Substitute.For<CallInvoker>();
        callInvokerMock.AsyncUnaryCall(default(Method<string, string>)!, default, default, default!)
            .ReturnsForAnyArgs(new AsyncUnaryCall<string>(
                Task.FromResult("Response"),
                Task.FromResult(Metadata.Empty),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }));

        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock, new []
        {
            clientFilterMockFirst,
            clientFilterMockSecond,
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        Assert.Equal("Response", result);
        callInvokerMock.Received();
        clientFilterMockFirst.Received();
        clientFilterMockSecond.Received();
        Assert.Equal(new[] { "First", "Second" }, calledFilters);
    }


    public interface IClientFilterTestService : IService<IClientFilterTestService>
    {
        UnaryResult<string> MethodA(string arg1);
    }
}
