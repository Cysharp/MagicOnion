using System.Collections.Concurrent;
using System.Reflection;
using Grpc.Core;
using MagicOnion.Serialization;

namespace MagicOnion.Server.Tests;

class FakeStreamingServiceContext<TRequest, TResponse> : IStreamingServiceContext<TRequest, TResponse>
{
    public bool IsStreamingHubCompleted { get; private set; }
    public List<TResponse> Responses { get; } = new List<TResponse>();

    public Guid ContextId => Guid.Empty;
    public DateTime Timestamp => DateTime.UnixEpoch;
    public Type ServiceType { get; }
    public MethodInfo MethodInfo { get; }
    public ILookup<Type, Attribute> AttributeLookup { get; }
    public MethodType MethodType => MethodType.DuplexStreaming;
    public ServerCallContext CallContext => throw new NotImplementedException();
    public IMagicOnionMessageSerializer MessageSerializer { get; }
    public IServiceProvider ServiceProvider { get; }
    public ConcurrentDictionary<string, object> Items { get; } = new ConcurrentDictionary<string, object>();
    public bool IsDisconnected => false;

    public FakeStreamingServiceContext(Type serviceType, MethodInfo methodInfo, IMagicOnionMessageSerializer messageSerializer, IServiceProvider serviceProvider, ILookup<Type, Attribute> attributeLookup = null)
    {
        ServiceType = serviceType;
        MethodInfo = methodInfo;
        MessageSerializer = messageSerializer;
        ServiceProvider = serviceProvider;

        AttributeLookup = attributeLookup ?? (new (Type, Attribute)[0]).ToLookup(k => k.Item1, v => v.Item2);
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
