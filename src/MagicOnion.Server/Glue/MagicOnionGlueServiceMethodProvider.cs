using Grpc.AspNetCore.Server.Model;

namespace MagicOnion.Server.Glue;

internal class MagicOnionGlueServiceMethodProvider<TService> : IServiceMethodProvider<TService>
    where TService : class
{
    private readonly MagicOnionServiceDefinition _magicOnionServiceDefinition;

    public MagicOnionGlueServiceMethodProvider(MagicOnionServiceDefinition magicOnionServerServiceDefinition)
    {
        _magicOnionServiceDefinition = magicOnionServerServiceDefinition ?? throw new ArgumentNullException(nameof(magicOnionServerServiceDefinition));
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
    {
        var binder = new MagicOnionGlueServiceBinder<TService>(context);
        foreach (var methodHandler in _magicOnionServiceDefinition.MethodHandlers)
        {
            methodHandler.BindHandler(binder);
        }
    }
}
