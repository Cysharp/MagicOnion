using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Server
{
    public static class MagicOnionEngine
    {
        public static ServerServiceDefinition BuildServerServiceDefinition()
        {
            return BuildServerServiceDefinition(new MagicOnionOptions());
        }

        public static ServerServiceDefinition BuildServerServiceDefinition(MagicOnionOptions option)
        {
            var builder = ServerServiceDefinition.CreateBuilder();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies
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
              })
              .Where(x => typeof(__IServiceMarker).IsAssignableFrom(x))
              .Where(x => !x.IsAbstract);

            // TODO:Parallel is 1 on Debug...
            Parallel.ForEach(types, new ParallelOptions { MaxDegreeOfParallelism = 1 }, classType =>
            {
                var className = classType.Name;
                if (!classType.GetConstructors().Any(x => x.GetParameters().Length == 0))
                {
                    throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", classType.FullName));
                }

                // TODO:Ignore
                // if (classType.GetCustomAttribute<IgnoreOperationAttribute>(true) != null) return; // ignore

                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue; // as property

                    // TODO:Ignore
                    // if (methodInfo.GetCustomAttribute<IgnoreOperationAttribute>(true) != null) continue; // ignore

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