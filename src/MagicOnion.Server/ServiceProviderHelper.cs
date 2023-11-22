using MagicOnion.Server.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server;

internal static class ServiceProviderHelper
{
    internal static TServiceBase CreateService<TServiceBase, TServiceInterface>(ServiceContext context)
        where TServiceBase : ServiceBase<TServiceInterface>
        where TServiceInterface : IServiceMarker
    {
        var instance = ActivatorUtilities.CreateInstance<TServiceBase>(context.ServiceProvider);
        instance.Context = context;
        instance.Metrics = context.ServiceProvider.GetRequiredService<MagicOnionMetrics>();
        return instance;
    }
}
