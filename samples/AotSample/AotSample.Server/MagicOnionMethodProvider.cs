using AotSample.Server.Services;
using MagicOnion;

namespace AotSample.Server;

/// <summary>
/// This partial class will be completed by the MagicOnion.Server.SourceGenerator.
/// It generates AOT-compatible method providers for the specified service types.
/// </summary>
[MagicOnionServerGeneration(typeof(GreeterService), typeof(ChatHub))]
public partial class MagicOnionMethodProvider
{
}
