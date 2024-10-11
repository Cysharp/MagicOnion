using System.Collections.Concurrent;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Serialization;
using NSubstitute;

namespace MagicOnion.Server.Tests;

class FakeStreamingServiceContext<TRequest, TResponse> : IStreamingServiceContext<TRequest, TResponse>
{
    public bool IsStreamingHubCompleted { get; private set; }
    public List<TResponse> Responses { get; } = new List<TResponse>();

    public Guid ContextId { get; } = Guid.NewGuid();
    public DateTime Timestamp => DateTime.UnixEpoch;
    public Type ServiceType { get; }
    public MethodInfo MethodInfo { get; }
    public ILookup<Type, Attribute> AttributeLookup { get; }
    public MethodType MethodType => MethodType.DuplexStreaming;
    public ServerCallContext CallContext { get; }
    public IMagicOnionSerializer MessageSerializer { get; }
    public IServiceProvider ServiceProvider { get; }
    public ConcurrentDictionary<string, object> Items { get; } = new ConcurrentDictionary<string, object>();
    public bool IsDisconnected => false;

    public FakeStreamingServiceContext(Type serviceType, MethodInfo methodInfo, IMagicOnionSerializer messageSerializer, IServiceProvider serviceProvider, ILookup<Type, Attribute> attributeLookup = null)
    {
        ServiceType = serviceType;
        MessageSerializer = messageSerializer;
        ServiceProvider = serviceProvider;

        AttributeLookup = attributeLookup ?? (new (Type, Attribute)[0]).ToLookup(k => k.Item1, v => v.Item2);

        var callContext = Substitute.For<ServerCallContext>();
        callContext.Method.Returns(methodInfo.Name);
        CallContext = callContext;
    }


    public void CompleteStreamingHub()
    {
        if (IsStreamingHubCompleted) throw new InvalidOperationException("StreamingHub has already been completed.");
        IsStreamingHubCompleted = true;
    }

    public IAsyncStreamReader<TRequest> RequestStream => throw new NotImplementedException();
    public IServerStreamWriter<TResponse> ResponseStream => throw new NotImplementedException();

    public void QueueResponseStreamWrite(in TResponse value)
    {
        lock (Responses)
        {
            Responses.Add(value);
        }
    }
}
