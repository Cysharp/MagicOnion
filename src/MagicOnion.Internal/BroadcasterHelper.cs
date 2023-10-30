using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicOnion.Internal
{
    internal static class BroadcasterHelper
    {
        internal static Type[] DynamicArgumentTupleTypes { get; } = typeof(DynamicArgumentTuple<,>)
            .GetTypeInfo()
            .Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        internal static MethodDefinition[] SearchDefinitions(Type interfaceType)
        {
            return interfaceType
                .GetInterfaces()
                .Concat(new[] { interfaceType })
                .SelectMany(x => x.GetMethods())
                .Where(x =>
                {
                    var methodInfo = x;
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) return false;
                    if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) return false; // ignore

                    var methodName = methodInfo.Name;
                    if (methodName == "Equals"
                     || methodName == "GetHashCode"
                     || methodName == "GetType"
                     || methodName == "ToString"
                     )
                    {
                        return false;
                    }

                    return true;
                })
                .Where(x => !x.IsSpecialName)
                .Select(x => new MethodDefinition(interfaceType, x, default))
                .ToArray();
        }

        internal static void VerifyMethodDefinitions(MethodDefinition[] definitions)
        {
            // define and set.
            var map = new Dictionary<int, MethodDefinition>(definitions.Length);
            foreach (var item in definitions)
            {
                var methodId = item.MethodInfo.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(item.MethodInfo.Name);
                if (map.ContainsKey(methodId))
                {
                    throw new Exception($"TReceiver does not allows duplicate methodId(hash code). Please change name or use MethodIdAttribute. {map[methodId].MethodInfo.Name} and {item.MethodInfo.Name}");
                }
                map.Add(methodId, item);

                if (!(item.MethodInfo.ReturnType == typeof(void)))
                {
                    throw new Exception($"Invalid definition, TReceiver's return type must only be `void`. {item.MethodInfo.Name}.");
                }

                item.MethodId = methodId;
            }
        }

        internal class MethodDefinition
        {
            public string Path => ReceiverType.Name + "/" + MethodInfo.Name;

            public Type ReceiverType { get; set; }
            public MethodInfo MethodInfo { get; set; }
            public int MethodId { get; set; }

            public MethodDefinition(Type receiverType, MethodInfo methodInfo, int methodId)
            {
                ReceiverType = receiverType;
                MethodInfo = methodInfo;
                MethodId = methodId;
            }
        }
    }
}
