using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

// Internal, Global.
internal static class ReflectionExtensions
{
    public static bool IsNullable(this System.Reflection.TypeInfo type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
    }

    public static bool IsPublic(this System.Reflection.TypeInfo type)
    {
        return type.IsPublic;
    }

}