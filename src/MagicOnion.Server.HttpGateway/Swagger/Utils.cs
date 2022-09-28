using System;
using System.Reflection;

namespace MagicOnion.Server.HttpGateway.Swagger;

internal static class Utils
{
    public static bool IsNullable(this Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}