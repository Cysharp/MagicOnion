using System;
using System.Collections.Generic;
using System.Linq;
using MagicOnion.Generator.Internal;
using MagicOnion.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.CodeAnalysis
{
    /// <summary>
    /// Provides logic to collect MagicOnion Services and StreamingHubs from a compilation.
    /// </summary>
    public class MethodCollector
    {
        readonly IMagicOnionGeneratorLogger logger;

        public MethodCollector(IMagicOnionGeneratorLogger logger = null)
        {
            this.logger = logger ?? MagicOnionGeneratorNullLogger.Instance;
        }

        public MagicOnionServiceCollection Collect(Compilation compilation)
        {
            var ctx = MethodCollectorContext.CreateFromCompilation(compilation, logger);

            return new MagicOnionServiceCollection(GetStreamingHubs(ctx), GetServices(ctx));
        }

        IReadOnlyList<MagicOnionStreamingHubInfo> GetStreamingHubs(MethodCollectorContext ctx)
        {
            return ctx.HubInterfaces
                .Select(x =>
                {
                    var serviceType = MagicOnionTypeInfo.CreateFromSymbol(x);
                    var ifDirective = x.GetDefinedGenerateIfCondition();
                    logger.Trace($"[{nameof(MethodCollectorContext)}] StreamingHub type '{serviceType.FullName}' (IfDirective={ifDirective})");

                    var methods = x.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(y => CreateHubMethodInfoFromMethodSymbol(serviceType, y))
                        .ToArray();

                    var receiverInterfaceSymbol = x.AllInterfaces.First(y => y.ConstructedFrom.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHub)).TypeArguments[1];
                    var receiverType = MagicOnionTypeInfo.CreateFromSymbol(receiverInterfaceSymbol);
                    var receiverMethods = receiverInterfaceSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(y => CreateHubReceiverMethodInfoFromMethodSymbol(serviceType, y))
                        .ToArray();

                    logger.Trace($"[{nameof(MethodCollectorContext)}] StreamingHub Receiver type '{receiverType.FullName}'");
                    var receiver = new MagicOnionStreamingHubInfo.MagicOnionStreamingHubReceiverInfo(receiverType, receiverMethods, receiverInterfaceSymbol.GetDefinedGenerateIfCondition());

                    return new MagicOnionStreamingHubInfo(serviceType, methods, receiver, ifDirective);
                })
                .OrderBy(x => x.ServiceType.FullName)
                .ToArray();
        }

        static int GetHubMethodIdFromMethodSymbol(IMethodSymbol methodSymbol)
            => (int?)methodSymbol.GetAttributes().FindAttributeShortName("MethodIdAttribute")?.ConstructorArguments[0].Value ?? FNV1A32.GetHashCode(methodSymbol.Name);

        MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo CreateHubMethodInfoFromMethodSymbol(MagicOnionTypeInfo interfaceType, IMethodSymbol methodSymbol)
        {
            var hubId = GetHubMethodIdFromMethodSymbol(methodSymbol);
            var methodReturnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
            var methodParameters = CreateParameterInfoListFromMethodSymbol(methodSymbol);
            var ifDirective = methodSymbol.GetDefinedGenerateIfCondition();
            var requestType = CreateRequestTypeFromMethodParameters(methodParameters);
            var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
            switch (methodReturnType.FullNameOpenType)
            {
                case "global::System.Threading.Tasks.Task":
                    //responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
                    break;
                case "global::System.Threading.Tasks.Task<>":
                    responseType = methodReturnType.GenericArguments[0];
                    break;
                default:
                    throw new InvalidOperationException($"StreamingHub method '{interfaceType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}' has unsupported return type '{methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}'.");
            }

            logger.Trace($"[{nameof(MethodCollectorContext)}] StreamingHub method '{methodSymbol.Name}' in type '{interfaceType.FullName}'");
            return new MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo(
                hubId,
                methodSymbol.Name,
                methodParameters,
                methodReturnType,
                requestType,
                responseType,
                ifDirective
            );
        }
        MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo CreateHubReceiverMethodInfoFromMethodSymbol(MagicOnionTypeInfo interfaceType, IMethodSymbol methodSymbol)
        {
            var hubId = GetHubMethodIdFromMethodSymbol(methodSymbol);
            var methodReturnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
            var methodParameters = CreateParameterInfoListFromMethodSymbol(methodSymbol);
            var ifDirective = methodSymbol.GetDefinedGenerateIfCondition();
            var requestType = CreateRequestTypeFromMethodParameters(methodParameters);
            var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
            if (methodReturnType != MagicOnionTypeInfo.KnownTypes.System_Void)
            {
                throw new InvalidOperationException($"StreamingHub receiver method '{interfaceType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}' has unsupported return type '{methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}'.");
            }

            logger.Trace($"[{nameof(MethodCollectorContext)}] StreamingHub receiver method '{methodSymbol.Name}' in type '{interfaceType.FullName}'");
            return new MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo(
                hubId,
                methodSymbol.Name,
                methodParameters,
                methodReturnType,
                requestType,
                responseType,
                ifDirective
            );
        }

        IReadOnlyList<MagicOnionServiceInfo> GetServices(MethodCollectorContext ctx)
        {
            return ctx.ServiceInterfaces
                .Select(x =>
                {
                    var serviceType = MagicOnionTypeInfo.CreateFromSymbol(x);
                    var ifDirective = x.GetDefinedGenerateIfCondition();
                    logger.Trace($"[{nameof(MethodCollectorContext)}] Service type '{serviceType.FullName}' (IfDirective={ifDirective})");

                    var methods = x.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(y => CreateServiceMethodInfoFromMethodSymbol(serviceType, y))
                        .ToArray();

                    return new MagicOnionServiceInfo(serviceType, methods, ifDirective);
                })
                .OrderBy(x => x.ServiceType.FullName)
                .ToArray();
        }
        
        MagicOnionServiceInfo.MagicOnionServiceMethodInfo CreateServiceMethodInfoFromMethodSymbol(MagicOnionTypeInfo serviceType, IMethodSymbol methodSymbol)
        {
            var methodReturnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
            var methodParameters = CreateParameterInfoListFromMethodSymbol(methodSymbol);
            var ifDirective = methodSymbol.GetDefinedGenerateIfCondition();
            var methodType = MethodType.Other;
            var requestType = CreateRequestTypeFromMethodParameters(methodParameters);
            var responseType = MagicOnionTypeInfo.KnownTypes.System_Void;
            switch (methodReturnType.FullNameOpenType)
            {
                case "global::MagicOnion.UnaryResult<>":
                    methodType = MethodType.Unary;
                    responseType = methodReturnType.GenericArguments[0];
                    break;
                case "global::System.Threading.Tasks.Task<>":
                    if (methodReturnType.HasGenericArguments)
                    {
                        switch (methodReturnType.GenericArguments[0].FullNameOpenType)
                        {
                            case "global::MagicOnion.ClientStreamingResult<,>":
                                methodType = MethodType.ClientStreaming;
                                requestType = methodReturnType.GenericArguments[0].GenericArguments[0];
                                responseType = methodReturnType.GenericArguments[0].GenericArguments[1];
                                break;
                            case "global::MagicOnion.ServerStreamingResult<>":
                                methodType = MethodType.ServerStreaming;
                                responseType = methodReturnType.GenericArguments[0].GenericArguments[0];
                                break;
                            case "global::MagicOnion.DuplexStreamingResult<,>":
                                methodType = MethodType.DuplexStreaming;
                                requestType = methodReturnType.GenericArguments[0].GenericArguments[0];
                                responseType = methodReturnType.GenericArguments[0].GenericArguments[1];
                                break;
                        }
                    }
                    break;
            }

            // Validates
            if (methodType == MethodType.Other)
            {
                throw new InvalidOperationException($"Unsupported return type '{methodReturnType.FullName}' ({serviceType.FullName}.{methodSymbol.Name})");
            }
            if (methodType == MethodType.Unary && responseType.Namespace == "MagicOnion" && (responseType.Name == "ClientStreamingResult" || responseType.Name == "ServerStreamingResult" || responseType.Name == "DuplexStreamingResult"))
            {
                throw new InvalidOperationException($"Unary methods can not return '{responseType.FullName}' ({serviceType.FullName}.{methodSymbol.Name})");
            }
            if ((methodType == MethodType.ClientStreaming || methodType == MethodType.DuplexStreaming) && methodParameters.Any())
            {
                throw new InvalidOperationException($"ClientStreaming and DuplexStreaming must have no parameters. ({serviceType.FullName}.{methodSymbol.Name})");
            }

            logger.Trace($"[{nameof(MethodCollectorContext)}] Service method '{methodSymbol.Name}' ({methodType}) in type '{serviceType.FullName}' (IfDirective={ifDirective})");
            return new MagicOnionServiceInfo.MagicOnionServiceMethodInfo(
                methodType,
                serviceType.Name,
                methodSymbol.Name,
                $"{serviceType.Name}/{methodSymbol.Name}",
                methodParameters,
                methodReturnType,
                requestType,
                responseType,
                ifDirective
            );
        }
        
        static IReadOnlyList<MagicOnionMethodParameterInfo> CreateParameterInfoListFromMethodSymbol(IMethodSymbol methodSymbol)
            => methodSymbol.Parameters.Select(x => MagicOnionMethodParameterInfo.CreateFromSymbol(x)).ToArray();

        static MagicOnionTypeInfo CreateRequestTypeFromMethodParameters(IReadOnlyList<MagicOnionMethodParameterInfo> parameters)
            => (parameters.Count == 0)
                ? MagicOnionTypeInfo.KnownTypes.MessagePack_Nil
                : (parameters.Count == 1)
                    ? parameters[0].Type
                    : MagicOnionTypeInfo.CreateValueType("MagicOnion", "DynamicArgumentTuple", parameters.Select(x => x.Type).ToArray());

        class MethodCollectorContext
        {
            public ReferenceSymbols ReferenceSymbols { get; }
            public IReadOnlyList<INamedTypeSymbol> ServiceAndHubInterfaces { get; }
            public IReadOnlyList<INamedTypeSymbol> ServiceInterfaces { get; }
            public IReadOnlyList<INamedTypeSymbol> HubInterfaces { get; }
            public IMagicOnionGeneratorLogger Logger { get; }

            public MethodCollectorContext(ReferenceSymbols referenceSymbols, IReadOnlyList<INamedTypeSymbol> serviceAndHubInterfaces, IMagicOnionGeneratorLogger logger)
            {
                ReferenceSymbols = referenceSymbols;
                ServiceAndHubInterfaces = serviceAndHubInterfaces;
                Logger = logger;

                var serviceInterfaces = new HashSet<INamedTypeSymbol>();
                var hubInterfaces = new HashSet<INamedTypeSymbol>();
                foreach (var type in serviceAndHubInterfaces)
                {
                    if (type.AllInterfaces.Any(y => y.ApproximatelyEqual(referenceSymbols.IStreamingHubMarker)))
                    {
                        // StreamingHub
                        if (!type.ConstructedFrom.ApproximatelyEqual(referenceSymbols.IStreamingHub))
                        {
                            hubInterfaces.Add(type);
                        }
                    }
                    else if (type.AllInterfaces.Any(y => y.ApproximatelyEqual(referenceSymbols.IServiceMarker)))
                    {
                        // Service
                        if (!type.ConstructedFrom.ApproximatelyEqual(referenceSymbols.IService))
                        {
                            serviceInterfaces.Add(type);
                        }
                    }
                }

                ServiceInterfaces = serviceInterfaces.ToArray();
                HubInterfaces = hubInterfaces.ToArray();
            }

            public static MethodCollectorContext CreateFromCompilation(Compilation compilation, IMagicOnionGeneratorLogger logger)
            {
                var typeReferences = new ReferenceSymbols(compilation, logger);
                var serviceAndHubInterfaces = compilation.GetNamedTypeSymbols()
                    .Where(x => x.TypeKind == TypeKind.Interface)
                    .Where(x =>
                    {
                        var all = x.AllInterfaces;
                        if (all.Any(y => y.ApproximatelyEqual(typeReferences.IServiceMarker)) || all.Any(y => y.ApproximatelyEqual(typeReferences.IStreamingHubMarker)))
                        {
                            return true;
                        }
                        return false;
                    })
                    .ToArray();

                return new MethodCollectorContext(typeReferences, serviceAndHubInterfaces, logger);
            }
        }
    }
}
