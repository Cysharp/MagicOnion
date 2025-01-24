using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Client.DynamicClient;

public class DynamicNotSupportedStreamingHubClientFactoryProvider : IStreamingHubClientFactoryProvider
{
    public static IStreamingHubClientFactoryProvider Instance { get; } = new DynamicNotSupportedStreamingHubClientFactoryProvider();

    DynamicNotSupportedStreamingHubClientFactoryProvider() { }

    public bool TryGetFactory<TStreamingHub, TReceiver>([NotNullWhen(true)] out StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver>? factory) where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        throw new NotSupportedException($"Unable to find a client factory of type '{typeof(TStreamingHub)}'. If the application is running on IL2CPP or AOT, the runtime and MagicOnion do not support dynamic code generation. Please use pre-generated code with Source Generator instead");
    }
}

[RequiresUnreferencedCode(nameof(DynamicStreamingHubClientFactoryProvider) + " is incompatible with trimming and Native AOT.")]
public class DynamicStreamingHubClientFactoryProvider : IStreamingHubClientFactoryProvider
{
    public static IStreamingHubClientFactoryProvider Instance { get; } = new DynamicStreamingHubClientFactoryProvider();

    DynamicStreamingHubClientFactoryProvider() { }

    public bool TryGetFactory<TStreamingHub, TReceiver>([NotNullWhen(true)] out StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver>? factory) where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        factory = Cache<TStreamingHub, TReceiver>.Factory;
        return true;
    }

    [RequiresUnreferencedCode(nameof(DynamicStreamingHubClientFactoryProvider) + "." + nameof(Cache<TStreamingHub, TReceiver>) + " is incompatible with trimming and Native AOT.")]
    static class Cache<TStreamingHub, TReceiver> where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
    {
        public static readonly StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver> Factory
            = (receiver, callInvoker, options) => (TStreamingHub)Activator.CreateInstance(DynamicStreamingHubClientBuilder<TStreamingHub, TReceiver>.ClientType, receiver, callInvoker, options)!;
    }
}
