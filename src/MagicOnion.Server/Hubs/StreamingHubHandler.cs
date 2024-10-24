using System.Reflection;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Binder;

namespace MagicOnion.Server.Hubs;

public class StreamingHubHandler : IEquatable<StreamingHubHandler>
{
    readonly IMagicOnionStreamingHubMethod hubMethod;
    readonly string toStringCache;
    readonly int getHashCodeCache;

    public string HubName => hubMethod.Metadata.StreamingHubInterfaceType.Name;
    public Type HubType => hubMethod.Metadata.StreamingHubImplementationType;
    public MethodInfo MethodInfo => hubMethod.Metadata.ImplementationMethod;
    public int MethodId => hubMethod.Metadata.MethodId;

    public ILookup<Type, Attribute> AttributeLookup => hubMethod.Metadata.AttributeLookup;

    internal Type RequestType => hubMethod.Metadata.RequestType;
    internal Func<StreamingHubContext, ValueTask> MethodBody { get; }

    public StreamingHubHandler(IMagicOnionStreamingHubMethod hubMethod, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
    {
        this.hubMethod = hubMethod;
        this.toStringCache = HubName + "/" + MethodInfo.Name;
        this.getHashCodeCache = HashCode.Combine(HubName, MethodInfo.Name);

        try
        {
            var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, hubMethod.Metadata.Attributes);
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
    public StreamingHubHandlerOptions(MagicOnionOptions options)
    {
        GlobalStreamingHubFilters = options.GlobalStreamingHubFilters;
    }
}

