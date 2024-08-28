using Grpc.AspNetCore.Server.Model;

namespace MagicOnion.Server.Binder;

internal class MagicOnionServiceMethodProvider<TService> : IServiceMethodProvider<TService>
    where TService : class
{
    readonly MagicOnionServiceDefinition magicOnionServiceDefinition;

    public MagicOnionServiceMethodProvider(MagicOnionServiceDefinition magicOnionServerServiceDefinition)
    {
        magicOnionServiceDefinition = magicOnionServerServiceDefinition ?? throw new ArgumentNullException(nameof(magicOnionServerServiceDefinition));
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
    {
        var binder = new MagicOnionServiceBinder<TService>(context);
        foreach (var methodHandler in magicOnionServiceDefinition.MethodHandlers)
        {
            methodHandler.BindHandler(binder);
        }
    }
}
