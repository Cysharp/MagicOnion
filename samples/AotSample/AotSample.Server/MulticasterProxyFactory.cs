using AotSample.Shared;
using Cysharp.Runtime.Multicast;

namespace AotSample.Server;

/// <summary>
/// This partial class will be completed by the Multicaster.SourceGenerator.
/// It generates AOT-compatible proxy factories for the specified receiver interface types.
/// </summary>
[MulticasterProxyGeneration(typeof(IChatHubReceiver))]
public partial class MulticasterProxyFactory
{
}
