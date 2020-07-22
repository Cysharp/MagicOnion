using Grpc.Core;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using MessagePack;

namespace MagicOnion.Server
{
    public static class MagicOnionEngine
    {
        /// <summary>
        /// Search MagicOnion service from all assemblies.
        /// </summary>
        /// <param name="isReturnExceptionStackTraceInErrorDetail">If true, when method body throws exception send to client exception.ToString message. It is useful for debugging.</param>
        /// <returns></returns>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, bool isReturnExceptionStackTraceInErrorDetail = false)
        {
            return BuildServerServiceDefinition(serviceProvider, new MagicOnionOptions() { IsReturnExceptionStackTraceInErrorDetail = isReturnExceptionStackTraceInErrorDetail });
        }

        /// <summary>
        /// Search MagicOnion service from all assemblies.
        /// </summary>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, MagicOnionOptions options)
        {
            return BuildServerServiceDefinition(serviceProvider, AppDomain.CurrentDomain.GetAssemblies(), options);
        }

        /// <summary>
        /// Search MagicOnion service from target assemblies. ex: new[]{ typeof(Startup).GetTypeInfo().Assembly }
        /// </summary>
        public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, Assembly[] searchAssemblies, MagicOnionOptions option)
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
            return BuildServerServiceDefinition(serviceProvider, types, option);
        }

        public static MagicOnionServiceDefinition BuildServerServiceDefinition(IServiceProvider serviceProvider, IEnumerable<Type> targetTypes, MagicOnionOptions option)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var handlers = new HashSet<MethodHandler>();
            var streamingHubHandlers = new List<StreamingHubHandler>();

            var types = targetTypes
              .Where(x => typeof(IServiceMarker).IsAssignableFrom(x))
              .Where(x => !x.GetTypeInfo().IsAbstract)
              .Where(x => x.GetCustomAttribute<IgnoreAttribute>(false) == null)
              .ToArray();

            option.MagicOnionLogger.BeginBuildServiceDefinition();
            var sw = Stopwatch.StartNew();

            try
            {
                foreach (var classType in types)
                {
                    var className = classType.Name;
                    if (!classType.GetConstructors().Any(x => x.GetParameters().Length == 0))
                    {
                        // supports paramaterless constructor after v2.1(DI support).
                        // throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", classType.FullName));
                    }

                    var isStreamingHub = typeof(IStreamingHubMarker).IsAssignableFrom(classType);
                    HashSet<StreamingHubHandler> tempStreamingHubHandlers = null;
                    if (isStreamingHub)
                    {
                        tempStreamingHubHandlers = new HashSet<StreamingHubHandler>();
                    }

                    var inheritInterface = classType.GetInterfaces()
                        .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == (isStreamingHub ? typeof(IStreamingHub<,>) : typeof(IService<>)))
                        .GenericTypeArguments[0];

                    if (!inheritInterface.IsAssignableFrom(classType))
                    {
                        throw new NotImplementedException($"Type '{classType.FullName}' has no implementation of interface '{inheritInterface.FullName}'.");
                    }

                    var interfaceMap = classType.GetInterfaceMap(inheritInterface);

                    for (int i = 0; i < interfaceMap.TargetMethods.Length; ++i)
                    {
                        var methodInfo = interfaceMap.TargetMethods[i];
                        var methodName = interfaceMap.InterfaceMethods[i].Name;

                        if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue;
                        if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) continue; // ignore

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
                            var streamingHandler = new StreamingHubHandler(classType, methodInfo, new StreamingHubHandlerOptions(option), serviceProvider);
                            if (!tempStreamingHubHandlers.Add(streamingHandler))
                            {
                                throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                            }
                            continue;
                        }
                        else
                        {
                            // create handler
                            var handler = new MethodHandler(classType, methodInfo, methodName, new MethodHandlerOptions(option), serviceProvider);
                            lock (builder)
                            {
                                if (!handlers.Add(handler))
                                {
                                    throw new InvalidOperationException($"Method does not allow overload, {className}.{methodName}");
                                }
                                handler.RegisterHandler(builder);
                            }
                        }
                    }

                    if (isStreamingHub)
                    {
                        var connectHandler = new MethodHandler(classType, classType.GetMethod("Connect"), "Connect", new MethodHandlerOptions(option), serviceProvider);
                        lock (builder)
                        {
                            if (!handlers.Add(connectHandler))
                            {
                                throw new InvalidOperationException($"Method does not allow overload, {className}.Connect");
                            }
                            connectHandler.RegisterHandler(builder);
                        }

                        lock (streamingHubHandlers)
                        {
                            streamingHubHandlers.AddRange(tempStreamingHubHandlers);
                            StreamingHubHandlerRepository.RegisterHandler(connectHandler, tempStreamingHubHandlers.ToArray());
                            IGroupRepositoryFactory factory;
                            var attr = classType.GetCustomAttribute<GroupConfigurationAttribute>(true);
                            if (attr != null)
                            {
                                factory = attr.Create();
                            }
                            else
                            {
                                factory = option.DefaultGroupRepositoryFactory;
                            }
                            StreamingHubHandlerRepository.AddGroupRepository(connectHandler, factory.CreateRepository(option.SerializerOptions, option.MagicOnionLogger));
                        }
                    }
                }
            }
            catch (AggregateException agex)
            {
                ExceptionDispatchInfo.Capture(agex.InnerExceptions[0]).Throw();
            }

            var result = new MagicOnionServiceDefinition(builder.Build(), handlers.ToArray(), streamingHubHandlers.ToArray());

            sw.Stop();
            option.MagicOnionLogger.EndBuildServiceDefinition(sw.Elapsed.TotalMilliseconds);

            return result;
        }
    }
}