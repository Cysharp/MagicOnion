using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public static class MagicOnionEngine
    {
        public static ServerServiceDefinition BuildServerServiceDefinition()
        {
            return BuildServerServiceDefinition(AppDomain.CurrentDomain.GetAssemblies(), new MagicOnionOptions());
        }

        public static ServerServiceDefinition BuildServerServiceDefinition(Assembly[] searchAssemblies, MagicOnionOptions option)
        {
            var types = searchAssemblies
              .SelectMany(x =>
              {
                  try
                  {
                      return x.GetTypes();
                  }
                  catch (ReflectionTypeLoadException ex)
                  {
                      return ex.Types.Where(t => t != null);
                  }
              });
            return BuildServerServiceDefinition(types, option);
        }

        public static ServerServiceDefinition BuildServerServiceDefinition(IEnumerable<Type> targetTypes, MagicOnionOptions option)
        {
            var builder = ServerServiceDefinition.CreateBuilder();

            var types = targetTypes
              .Where(x => typeof(__IServiceMarker).IsAssignableFrom(x))
              .Where(x => !x.IsAbstract)
              .Where(x => x.GetCustomAttribute<IgnoreAttribute>(false) == null)
              .ToArray();

            Parallel.ForEach(types, new ParallelOptions { MaxDegreeOfParallelism = 1 }, classType =>
            {
                var className = classType.Name;
                if (!classType.GetConstructors().Any(x => x.GetParameters().Length == 0))
                {
                    throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", classType.FullName));
                }

                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue;
                    if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) continue; // ignore

                    var methodName = methodInfo.Name;

                    // ignore default methods
                    if (methodName == "Equals"
                     || methodName == "GetHashCode"
                     || methodName == "GetType"
                     || methodName == "ToString"
                     || methodName == "WithOption"
                     || methodName == "WithHeaders"
                     || methodName == "WithDeadline"
                     || methodName == "WithCancellationToken"
                     || methodName == "WithHost"
                     )
                    {
                        continue;
                    }

                    var sw = Stopwatch.StartNew();

                    // create handler
                    var handler = new MethodHandler(option, classType, methodInfo);
                    lock (builder)
                    {
                        handler.RegisterHandler(builder);
                    }

                    sw.Stop();
                }
            });

            return builder.Build();
        }
    }
}