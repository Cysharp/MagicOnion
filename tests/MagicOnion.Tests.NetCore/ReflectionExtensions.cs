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

    public static bool IsConstructedGenericType(this System.Reflection.TypeInfo type)
    {
        return type.AsType().IsConstructedGenericType;
    }

    public static MethodInfo GetGetMethod(this PropertyInfo propInfo)
    {
        return propInfo.GetMethod;
    }

    public static MethodInfo GetSetMethod(this PropertyInfo propInfo)
    {
        return propInfo.SetMethod;
    }

    public static bool IsEnum(this Type type)
    {
        return type.GetTypeInfo().IsEnum;
    }

    public static bool IsAbstract(this Type type)
    {
        return type.GetTypeInfo().IsAbstract;
    }

    public static Type[] GetGenericArguments(this Type type)
    {
        return type.GetTypeInfo().GetGenericArguments();
    }

    public static ConstructorInfo[] GetConstructors(this Type type)
    {
        return type.GetTypeInfo().GetConstructors();
    }

    public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
    {
        return type.GetTypeInfo().GetMethods(flags);
    }

    public static T GetCustomAttribute<T>(this Type type, bool inherit)
        where T : Attribute
    {
        return type.GetTypeInfo().GetCustomAttribute<T>(inherit);
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
        where T : Attribute
    {
        return type.GetTypeInfo().GetCustomAttributes<T>(inherit);
    }

    public static IEnumerable<Attribute> GetCustomAttributes(this Type type, bool inherit)
    {
        return type.GetTypeInfo().GetCustomAttributes(inherit).Cast<Attribute>();
    }

    public static bool IsAssignableFrom(this Type type, Type c)
    {
        return type.GetTypeInfo().IsAssignableFrom(c);
    }

    public static PropertyInfo GetProperty(this Type type, string name)
    {
        return type.GetTypeInfo().GetProperty(name);
    }

    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
    {
        return type.GetTypeInfo().GetProperty(name, flags);
    }

    public static FieldInfo GetField(this Type type, string name, BindingFlags flags)
    {
        return type.GetTypeInfo().GetField(name, flags);
    }

    public static FieldInfo[] GetFields(this Type type, BindingFlags flags)
    {
        return type.GetTypeInfo().GetFields(flags);
    }

    public static Type GetInterface(this Type type, string name)
    {
        return type.GetTypeInfo().GetInterface(name);
    }

    public static Type[] GetInterfaces(this Type type)
    {
        return type.GetTypeInfo().GetInterfaces();
    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
        return type.GetTypeInfo().GetMethod(name);
    }

    public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
    {
        return type.GetTypeInfo().GetMethod(name, flags);
    }

    public static MethodInfo[] GetMethods(this Type type)
    {
        return type.GetTypeInfo().GetMethods();
    }

    public static ConstructorInfo GetConstructor(this Type type, Type[] types)
    {
        return type.GetTypeInfo().GetConstructor(types);
    }

    public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingAttr, object dymmy1, Type[] types, object dummy2)
    {
        return type.GetTypeInfo().GetConstructors(bindingAttr).First(x =>
        {
            return x.GetParameters().Select(y => y.ParameterType).SequenceEqual(types);
        });
    }

    public static object InvokeMember(this Type type, string name, BindingFlags invokeAttr, object dummy, object target, object[] args)
    {
        return type.GetTypeInfo().GetMethod(name, invokeAttr).Invoke(target, args);
    }
}