using System;
using System.Collections.Generic;
using System.Reflection;

namespace MagicOnion.Server
{
    internal static class TypeExtensions
    {
        private static IEnumerable<Type> InterfaceWithParents(Type interfaceType)
        {
            if (interfaceType.IsInterface)
                yield return interfaceType;
            foreach (var parentInterface in interfaceType.GetInterfaces())
            {
                if (!ForbiddenInterface(parentInterface))
                    yield return parentInterface;
            }

            //skip some marker interfaces
            static bool ForbiddenInterface(Type parent)
            {
                if (!parent.IsGenericType)
                    return false;
                var parentGeneric = parent.GetGenericTypeDefinition();
                return parentGeneric == typeof(IStreamingHub<,>) || parentGeneric == typeof(IService<>);
            }
        }

        /// <summary>
        /// Returns an interface mapping for the specified interface type and
        /// all of its parents.
        /// </summary>
        /// <returns>An object that represents the interface mapping for interfaceType.</returns>
        public static InterfaceMapping GetInterfaceMapWithParents(this Type targetType, Type interfaceType)
        {
            var interfaceMethods = new List<MethodInfo>();
            var targetMethods = new List<MethodInfo>();
            foreach (var currentInterface in InterfaceWithParents(interfaceType))
            {
                var map = targetType.GetInterfaceMap(currentInterface);
                interfaceMethods.AddRange(map.InterfaceMethods);
                targetMethods.AddRange(map.TargetMethods);
            }

            return new InterfaceMapping
            {
                InterfaceMethods = interfaceMethods.ToArray(),
                TargetMethods = targetMethods.ToArray(),
                TargetType = targetType,
                InterfaceType = interfaceType
            };
        }
    }
}
