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
        /// <summary>
        /// Search MagicOnion service from all assemblies.
        /// </summary>
        /// <param name="isReturnExceptionStackTraceInErrorDetail">If true, when method body throws exception send to client exception.ToString message. It is useful for debugging.</param>
        /// <returns></returns>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(bool isReturnExceptionStackTraceInErrorDetail = false)
        {
            return BuildServerServiceDefinition(new MagicOnionOptions(isReturnExceptionStackTraceInErrorDetail));
        }

        /// <summary>
        /// Search MagicOnion service from all assemblies.
        /// </summary>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
        {
            return BuildServerServiceDefinition(AppDomain.CurrentDomain.GetAssemblies(), options);
        }

        public static MagicOnionServiceDefinition BuildServerServiceDefinition(Assembly[] searchAssemblies, MagicOnionOptions option)
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

        public static MagicOnionServiceDefinition BuildServerServiceDefinition(IEnumerable<Type> targetTypes, MagicOnionOptions option)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var handlers = new HashSet<MethodHandler>();

            var types = targetTypes
              .Where(x => typeof(IServiceMarker).IsAssignableFrom(x))
              .Where(x => !x.IsAbstract)
              .Where(x => x.GetCustomAttribute<IgnoreAttribute>(false) == null)
              .Concat(SupplyEmbeddedServices(option))
              .ToArray();

            option.MagicOnionLogger.BeginBuildServiceDefinition();
            var sw = Stopwatch.StartNew();

            Parallel.ForEach(types, /*new ParallelOptions { MaxDegreeOfParallelism = 1 },*/ classType =>
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
                     || methodName == "WithOptions"
                     || methodName == "WithHeaders"
                     || methodName == "WithDeadline"
                     || methodName == "WithCancellationToken"
                     || methodName == "WithHost"
                     )
                    {
                        continue;
                    }

                    // create handler
                    var handler = new MethodHandler(option, classType, methodInfo);
                    lock (builder)
                    {
                        if (!handlers.Add(handler))
                        {
                            throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                        }
                        handler.RegisterHandler(builder);
                    }
                }
            });

            var result = new MagicOnionServiceDefinition(builder.Build(), handlers.ToArray());

            sw.Stop();
            option.MagicOnionLogger.EndBuildServiceDefinition(sw.Elapsed.TotalMilliseconds);

            return result;
        }

        static IEnumerable<Type> SupplyEmbeddedServices(MagicOnionOptions options)
        {
            if (options.DisableEmbeddedService) yield break;

            yield return typeof(MagicOnion.Server.EmbeddedServices.MagicOnionEmbeddedHeartbeat);
            yield return typeof(MagicOnion.Server.EmbeddedServices.MagicOnionEmbeddedPing);
        }
    }
}