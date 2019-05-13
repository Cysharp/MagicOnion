using MagicOnion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
    public static class BroadcasterHelper
    {
        internal static readonly Type[] dynamicArgumentTupleTypes = typeof(DynamicArgumentTuple<,>).GetTypeInfo().Assembly
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
                .Select(x => new MethodDefinition
                {
                    ReceiverType = interfaceType,
                    MethodInfo = x,
                })
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

        public static async void FireAndForget(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                Grpc.Core.GrpcEnvironment.Logger?.Error(ex, "exception occured in client broadcast.");
            }
        }

        internal class MethodDefinition
        {
            public string Path => ReceiverType.Name + "/" + MethodInfo.Name;

            public Type ReceiverType;
            public MethodInfo MethodInfo;
            public int MethodId;
        }
    }
}