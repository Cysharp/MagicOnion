namespace MagicOnion.Client.Tests;

public class ClientFilterTest
{
    [Fact]
    public async Task ReturnImmediateValue()
    {
        // Arrange
        var clientFilterMock = new Mock<IClientFilter>();
        clientFilterMock.Setup(x => x.SendAsync(It.IsAny<RequestContext>(), It.IsAny<Func<RequestContext, ValueTask<ResponseContext>>>()))
            // NOTE: Mock IClientFilter returns a value immediately. (The filter will not call `next`)
            .Returns(ValueTask.FromResult((ResponseContext)new ResponseContext<string>("Response", Status.DefaultSuccess, Metadata.Empty, Metadata.Empty)))
            .Verifiable();
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()))
            .Returns(new AsyncUnaryCall<string>(
                Task.FromResult("Response"),
                Task.FromResult(Metadata.Empty),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }))
            .Verifiable();
        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock.Object, new []
        {
            clientFilterMock.Object,
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        result.Should().Be("Response");
        callInvokerMock.Verify(x => x.AsyncUnaryCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()), Times.Never);
        clientFilterMock.Verify();
    }

    [Fact]
    public async Task RequestHeaders()
    {
        // Arrange
        var callInvokerMock = new Mock<CallInvoker>();
        var requestHeaders = default(Metadata);
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()))
            .Callback<Method<string, string>, string, CallOptions, string>((method, host, callOptions, request) =>
            {
                requestHeaders = callOptions.Headers;
            })
            .Returns(new AsyncUnaryCall<string>(
                Task.FromResult("Response"),
                Task.FromResult(Metadata.Empty),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }));
        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock.Object, new []
        {
            new ClientFilterTestRequestHeaders(),
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        result.Should().Be("Response");
        requestHeaders.Should().HaveCount(2);
        requestHeaders.Should().Contain(x => x.Key == "x-header-1" && x.Value == "valueA");
        requestHeaders.Should().Contain(x => x.Key == "x-header-2" && x.Value == "valueB");
    }

    class ClientFilterTestRequestHeaders : IClientFilter
    {
        public ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            context.CallOptions.Headers.Add("x-header-1", "valueA");
            context.CallOptions.Headers.Add("x-header-2", "valueB");
            return next(context);
        }
    }

    [Fact]
    public async Task FilterChain()
    {
        // Arrange
        var calledFilters = new List<string>();
        var clientFilterMockFirst = new Mock<IClientFilter>();
        clientFilterMockFirst.Setup(x => x.SendAsync(It.IsAny<RequestContext>(), It.IsAny<Func<RequestContext, ValueTask<ResponseContext>>>()))
            .Returns<RequestContext, Func<RequestContext, ValueTask<ResponseContext>>>((context, next) =>
            {
                calledFilters.Add("First");
                return next(context);
            })
            .Verifiable();
        var clientFilterMockSecond = new Mock<IClientFilter>();
        clientFilterMockSecond.Setup(x => x.SendAsync(It.IsAny<RequestContext>(), It.IsAny<Func<RequestContext, ValueTask<ResponseContext>>>()))
            .Returns<RequestContext, Func<RequestContext, ValueTask<ResponseContext>>>((context, next) =>
            {
                calledFilters.Add("Second");
                return next(context);
            })
            .Verifiable();
        var callInvokerMock = new Mock<CallInvoker>();
        callInvokerMock.Setup(x => x.AsyncUnaryCall(It.IsAny<Method<string, string>>(), It.IsAny<string>(), It.IsAny<CallOptions>(), It.IsAny<string>()))
            .Returns(new AsyncUnaryCall<string>(
                Task.FromResult("Response"),
                Task.FromResult(Metadata.Empty),
                () => Status.DefaultSuccess,
                () => Metadata.Empty,
                () => { }))
            .Verifiable();
        var client = MagicOnionClient.Create<IClientFilterTestService>(callInvokerMock.Object, new []
        {
            clientFilterMockFirst.Object,
            clientFilterMockSecond.Object,
        });

        // Act
        var result = await client.MethodA("Request");

        // Assert
        result.Should().Be("Response");
        callInvokerMock.Verify();
        clientFilterMockFirst.Verify();
        clientFilterMockSecond.Verify();
        calledFilters.Should().BeEquivalentTo(new[] { "First", "Second" });
    }


    public interface IClientFilterTestService : IService<IClientFilterTestService>
    {
        UnaryResult<string> MethodA(string arg1);
    }
}
