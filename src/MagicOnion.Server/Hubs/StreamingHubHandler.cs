using System.Buffers;
using MessagePack;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Internal;
using MagicOnion.Serialization;
using MagicOnion.Server.Binder;

namespace MagicOnion.Server.Hubs;

public class StreamingHubHandler : IEquatable<StreamingHubHandler>
{
    readonly StreamingHubMethodHandlerMetadata metadata;
    readonly string toStringCache;
    readonly int getHashCodeCache;

    public string HubName => metadata.StreamingHubInterfaceType.Name;
    public Type HubType => metadata.StreamingHubImplementationType;
    public MethodInfo MethodInfo => metadata.ImplementationMethod;
    public int MethodId => metadata.MethodId;

    public ILookup<Type, Attribute> AttributeLookup => metadata.AttributeLookup;

    internal Type RequestType => metadata.RequestType;
    internal Func<StreamingHubContext, ValueTask> MethodBody { get; }

    public StreamingHubHandler(Type implementationType, IMagicOnionStreamingHubMethod hubMethod, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
    {
        this.metadata = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata(implementationType, hubMethod.MethodInfo);
        this.toStringCache = HubName + "/" + MethodInfo.Name;
        this.getHashCodeCache = HashCode.Combine(HubName, MethodInfo.Name);

        try
        {
            var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, implementationType, hubMethod.MethodInfo);
            this.MethodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, hubMethod.InvokeAsync);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
        }
    }

    public override string ToString()
        => toStringCache;

    public override int GetHashCode()
        => getHashCodeCache;

    public bool Equals(StreamingHubHandler? other)
        => other != null && HubName.Equals(other.HubName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
}

/// <summary>
/// Options for StreamingHubHandler construction.
/// </summary>
public class StreamingHubHandlerOptions
{
    public IList<StreamingHubFilterDescriptor> GlobalStreamingHubFilters { get; }

    public IMagicOnionSerializerProvider MessageSerializer { get; }

    public StreamingHubHandlerOptions(MagicOnionOptions options)
    {
        GlobalStreamingHubFilters = options.GlobalStreamingHubFilters;
        MessageSerializer = options.MessageSerializer;
    }
}

