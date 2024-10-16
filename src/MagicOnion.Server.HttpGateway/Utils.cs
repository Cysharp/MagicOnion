using System.Reflection;

namespace MagicOnion;

internal static class Utils
{
    private static readonly NullabilityInfoContext nullabilityInfoContext = new();
    public static bool IsNullable(this ParameterInfo type)
    {
        return nullabilityInfoContext.Create(type).WriteState == NullabilityState.Nullable;
    }

    public static bool IsNullable(this Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
