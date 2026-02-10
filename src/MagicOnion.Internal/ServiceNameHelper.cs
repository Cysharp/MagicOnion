using System.Reflection;

namespace MagicOnion.Internal;

internal static class ServiceNameHelper
{
    /// <summary>
    /// Resolves the gRPC service name for a given service interface type.
    /// If the interface has a <see cref="ServiceNameAttribute"/>, its value is used.
    /// Otherwise, the short type name (<see cref="Type.Name"/>) is used as the default.
    /// </summary>
    /// <param name="serviceInterfaceType">The service interface type (e.g., IMyService).</param>
    /// <returns>The resolved service name string.</returns>
    public static string GetServiceName(Type serviceInterfaceType)
    {
        var attr = serviceInterfaceType.GetCustomAttribute<ServiceNameAttribute>();
        if (attr is not null)
        {
            return attr.Name;
        }

        return serviceInterfaceType.Name;
    }
}
