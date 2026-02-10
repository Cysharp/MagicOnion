using System.Reflection;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Binder;

/* Unmerged change from project 'MagicOnion.Server (net8.0)'
Added:
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using MagicOnion.Server.Hubs.Internal;
*/

namespace MagicOnion.Server.Hubs.Internal;

internal class StreamingHubHandler : IEquatable<StreamingHubHandler>
{
    readonly IMagicOnionStreamingHubMethod hubMethod;
    readonly string toStringCache;
    readonly int getHashCodeCache;

    public string HubName => hubMethod.ServiceName;
    public Type HubType => hubMethod.Metadata.StreamingHubImplementationType;
    public MethodInfo MethodInfo => hubMethod.Metadata.ImplementationMethod;
    public int MethodId => hubMethod.Metadata.MethodId;

    public ILookup<Type, Attribute> AttributeLookup => hubMethod.Metadata.AttributeLookup;

    internal Type RequestType => hubMethod.Metadata.RequestType;
    internal Func<StreamingHubContext, ValueTask> MethodBody { get; }

    public StreamingHubHandler(IMagicOnionStreamingHubMethod hubMethod, StreamingHubHandlerOptions handlerOptions, IServiceProvider serviceProvider)
    {
        this.hubMethod = hubMethod;
        toStringCache = HubName + "/" + MethodInfo.Name;
        getHashCodeCache = HashCode.Combine(HubName, MethodInfo.Name);

        try
        {
            var filters = FilterHelper.GetFilters(handlerOptions.GlobalStreamingHubFilters, hubMethod.Metadata.Metadata.OfType<Attribute>());
            MethodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, hubMethod.InvokeAsync);
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

