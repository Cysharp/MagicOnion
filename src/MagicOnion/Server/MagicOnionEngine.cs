using Grpc.Core;
using MagicOnion.Server.Hubs;
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

#if NET_FRAMEWORK

        /// <summary>
        /// Search MagicOnion service from all assemblies.
        /// </summary>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
        {
            return BuildServerServiceDefinition(AppDomain.CurrentDomain.GetAssemblies(), options);
        }

#else

        /// <summary>
        /// Search MagicOnion service from entry assembly.
        /// </summary>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(MagicOnionOptions options)
        {
            return BuildServerServiceDefinition(new[] { Assembly.GetEntryAssembly() }, options);
        }

#endif

        /// <summary>
        /// Search MagicOnion service from target assemblies. ex: new[]{ typeof(Startup).GetTypeInfo().Assembly }
        /// </summary>
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
            var streamingHubHandlers = new List<StreamingHubHandler>();

            var types = targetTypes
              .Where(x => typeof(IServiceMarker).IsAssignableFrom(x))
              .Where(x => !x.GetTypeInfo().IsAbstract)
              .Where(x => x.GetCustomAttribute<IgnoreAttribute>(false) == null)
              .Concat(SupplyEmbeddedServices(option))
              .ToArray();

            option.MagicOnionLogger.BeginBuildServiceDefinition();
            var sw = Stopwatch.StartNew();

            Parallel.ForEach(types, /* new ParallelOptions { MaxDegreeOfParallelism = 1 }, */ classType =>
            {
                var className = classType.Name;
                if (!classType.GetConstructors().Any(x => x.GetParameters().Length == 0))
                {
                    throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", classType.FullName));
                }

                var isStreamingHub = typeof(IStreamingHubMarker).IsAssignableFrom(classType);
                HashSet<StreamingHubHandler> tempStreamingHubHandlers = null;
                MethodHandler tempParentStreamingMethodHandler = null;
                if (isStreamingHub)
                {
                    tempStreamingHubHandlers = new HashSet<StreamingHubHandler>();
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

                    // register for StreamingHub
                    if (isStreamingHub && methodName != "Connect")
                    {
                        var streamingHandler = new StreamingHubHandler(option, classType, methodInfo);
                        if (!tempStreamingHubHandlers.Add(streamingHandler))
                        {
                            throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                        }
                        continue;
                    }
                    else
                    {
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

                        if (isStreamingHub && methodName == "Connect")
                        {
                            tempParentStreamingMethodHandler = handler;
                        }
                    }
                }

                if (isStreamingHub)
                {
                    lock (streamingHubHandlers)
                    {
                        streamingHubHandlers.AddRange(tempStreamingHubHandlers);
                        StreamingHubHandlerRepository.RegisterHandler(tempParentStreamingMethodHandler, tempStreamingHubHandlers.ToArray());
                        // TODO:ConfigureFactory
                        StreamingHubHandlerRepository.AddGroupRepository(tempParentStreamingMethodHandler, option.DefaultGroupRepositoryFactory.CreateRepository());
                    }
                }
            });

            var result = new MagicOnionServiceDefinition(builder.Build(), handlers.ToArray(), streamingHubHandlers.ToArray());

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