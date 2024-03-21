using Grpc.Net.Client;
using MagicOnion.Client;

namespace MagicOnion.Integration.Tests.StreamingHubClientDiagnosticHandler;

public class StreamingHubClientDiagnosticHandlerTest : IClassFixture<MagicOnionApplicationFactory<StreamingHubTestHub>>
{
    readonly MagicOnionApplicationFactory<StreamingHubTestHub> factory;

    public StreamingHubClientDiagnosticHandlerTest(MagicOnionApplicationFactory<StreamingHubTestHub> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Request_Response()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var diagnosticHandler = new DiagnosticHandler();

        MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubDiagnosticHandler = diagnosticHandler;

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(
            channel, receiver,
            factoryProvider: MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubClientFactoryProvider);

        // Act
        var result = await client.Parameter_Many(12345, "Hello✨", true);

        // Assert
        Assert.Equal([DiagnosticHandler.EventType.OnRequestBegin, DiagnosticHandler.EventType.OnRequestEnd], diagnosticHandler.Events.Select(x => x.EventType));

        var beginEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestBegin);
        var endEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestEnd);

        Assert.Equal(nameof(IStreamingHubTestHub.Parameter_Many), beginEvent.MethodName);
        Assert.Equal(nameof(IStreamingHubTestHub.Parameter_Many), endEvent.MethodName);
        Assert.Equal(new DynamicArgumentTuple<int, string, bool>(12345, "Hello✨", true), beginEvent.Request);
        Assert.Equal(result, endEvent.Response);
    }

    [Fact]
    public async Task Request_Parameterless_Response()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var diagnosticHandler = new DiagnosticHandler();

        MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubDiagnosticHandler = diagnosticHandler;

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(
            channel, receiver,
            factoryProvider: MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubClientFactoryProvider);

        // Act
        var result = await client.Parameter_Zero();

        // Assert
        Assert.Equal([DiagnosticHandler.EventType.OnRequestBegin, DiagnosticHandler.EventType.OnRequestEnd], diagnosticHandler.Events.Select(x => x.EventType));

        var beginEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestBegin);
        var endEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestEnd);

        Assert.Equal(nameof(IStreamingHubTestHub.Parameter_Zero), beginEvent.MethodName);
        Assert.Equal(nameof(IStreamingHubTestHub.Parameter_Zero), endEvent.MethodName);
        Assert.Equal(Nil.Default, beginEvent.Request);
        Assert.Equal(result, endEvent.Response);
    }

    [Fact]
    public async Task Request_Response_Void()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var diagnosticHandler = new DiagnosticHandler();

        MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubDiagnosticHandler = diagnosticHandler;

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(
            channel, receiver,
            factoryProvider: MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubClientFactoryProvider);

        // Act
        await client.NoReturn_Parameter_Many(12345, "Hello✨", true);

        // Assert
        Assert.Equal([DiagnosticHandler.EventType.OnRequestBegin, DiagnosticHandler.EventType.OnRequestEnd], diagnosticHandler.Events.Select(x => x.EventType));

        var beginEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestBegin);
        var endEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestEnd);

        Assert.Equal(nameof(IStreamingHubTestHub.NoReturn_Parameter_Many), beginEvent.MethodName);
        Assert.Equal(nameof(IStreamingHubTestHub.NoReturn_Parameter_Many), endEvent.MethodName);
        Assert.Equal(new DynamicArgumentTuple<int, string, bool>(12345, "Hello✨", true), beginEvent.Request);
        Assert.Equal(Nil.Default, endEvent.Response);
    }

    [Fact]
    public async Task Request_Throw()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var diagnosticHandler = new DiagnosticHandler();

        MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubDiagnosticHandler = diagnosticHandler;

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(
            channel, receiver,
            factoryProvider: MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubClientFactoryProvider);

        // Act
        var ex = await Record.ExceptionAsync(async () => await client.Throw());

        // Assert
        Assert.Equal([DiagnosticHandler.EventType.OnRequestBegin, DiagnosticHandler.EventType.OnRequestEnd], diagnosticHandler.Events.Select(x => x.EventType));

        var beginEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestBegin);
        var endEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestEnd);

        Assert.Equal(nameof(IStreamingHubTestHub.Throw), beginEvent.MethodName);
        Assert.Equal(nameof(IStreamingHubTestHub.Throw), endEvent.MethodName);
        Assert.Equal(Nil.Default, beginEvent.Request);
        Assert.Null(endEvent.Response);
        Assert.Equal(ex, endEvent.Exception);
    }

    [Fact]
    public async Task Receiver()
    {
        // Arrange
        var httpClient = factory.CreateDefaultClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions() { HttpClient = httpClient });
        var diagnosticHandler = new DiagnosticHandler();

        MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubDiagnosticHandler = diagnosticHandler;

        var receiver = Substitute.For<IStreamingHubTestHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IStreamingHubTestHub, IStreamingHubTestHubReceiver>(
            channel, receiver,
            factoryProvider: MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler.StreamingHubClientFactoryProvider);

        // Act
        await client.CallReceiver_Parameter_Many(12345, "Hello✨", true);
        await Task.Delay(500); // Wait for broadcast queue to be consumed.

        // Assert
        receiver.Received().Receiver_Parameter_Many(12345, "Hello✨", true);
        Assert.Contains(DiagnosticHandler.EventType.OnRequestBegin, diagnosticHandler.Events.Select(x => x.EventType));
        Assert.Contains(DiagnosticHandler.EventType.OnRequestEnd, diagnosticHandler.Events.Select(x => x.EventType));
        Assert.Contains(DiagnosticHandler.EventType.OnBroadcastEvent, diagnosticHandler.Events.Select(x => x.EventType));

        var beginEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestBegin);
        var endEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnRequestEnd);
        var broadcastEvent = diagnosticHandler.Events.Single(x => x.EventType == DiagnosticHandler.EventType.OnBroadcastEvent);

        Assert.Equal(nameof(IStreamingHubTestHub.CallReceiver_Parameter_Many), beginEvent.MethodName);
        Assert.Equal(nameof(IStreamingHubTestHub.CallReceiver_Parameter_Many), endEvent.MethodName);
        Assert.Equal(new DynamicArgumentTuple<int, string, bool>(12345, "Hello✨", true), beginEvent.Request);
        Assert.Null(beginEvent.Response);

        Assert.Equal(nameof(IStreamingHubTestHubReceiver.Receiver_Parameter_Many), broadcastEvent.MethodName);
        Assert.Equal(new DynamicArgumentTuple<int, string, bool>(12345, "Hello✨", true), broadcastEvent.Response);
    }

    class DiagnosticHandler : IStreamingHubDiagnosticHandler
    {
        public enum EventType
        {
            OnRequestBegin,
            OnRequestEnd,
            OnBroadcastEvent,
        }

        public List<(EventType EventType, object HubInstance, string MethodName, object? Request, object? Response, Exception? Exception)> Events { get; } = new ();

        public async Task<TResponse> OnMethodInvoke<THub, TRequest, TResponse>(THub hubInstance, int methodId, string methodName, TRequest request, bool isFireAndForget, IStreamingHubDiagnosticHandler.InvokeMethodDelegate<TRequest, TResponse> invokeMethod)
        {
            Events.Add((EventType.OnRequestBegin, hubInstance!, methodName, request, default, default));
            try
            {
                var result = await invokeMethod(methodId, request).ConfigureAwait(false);
                Events.Add((EventType.OnRequestEnd, hubInstance!, methodName, default, result, default));
                return result;
            }
            catch (Exception e)
            {
                Events.Add((EventType.OnRequestEnd, hubInstance!, methodName, default, default, e));
                throw;
            }
        }

        public void OnBroadcastEvent<THub, T>(THub hubInstance, string methodName, T value)
        {
            Events.Add((EventType.OnBroadcastEvent, hubInstance!, methodName, default, value, default));
        }
    }
}


[MagicOnionClientGeneration(typeof(MagicOnionGeneratedClientInitializer), DisableAutoRegistration = true, EnableStreamingHubDiagnosticHandler = true, GenerateFileHintNamePrefix = "SD_")]
public partial class MagicOnionGeneratedClientInitializerStreamingHubDiagnosticHandler
{ }
