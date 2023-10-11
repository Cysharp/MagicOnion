using System.Collections.Immutable;
using MagicOnion.Client.SourceGenerator.Internal;
using MagicOnion.Client.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

/// <summary>
/// Provides logic to collect MagicOnion Services and StreamingHubs from a compilation.
/// </summary>
public class MethodCollector
{
    readonly CancellationToken cancellationToken;

    public MethodCollector(CancellationToken cancellationToken = default)
    {
        this.cancellationToken = cancellationToken;
    }

    public MagicOnionServiceCollection Collect(ImmutableArray<INamedTypeSymbol> interfaceSymbols, ReferenceSymbols referenceSymbols)
    {
        var ctx = MethodCollectorContext.CreateFromInterfaceSymbols(interfaceSymbols, referenceSymbols);

        return new MagicOnionServiceCollection(GetStreamingHubs(ctx), GetServices(ctx));
    }

    IReadOnlyList<MagicOnionStreamingHubInfo> GetStreamingHubs(MethodCollectorContext ctx)
    {
        return ctx.HubInterfaces
            .Select(x =>
            {
                var serviceType = MagicOnionTypeInfo.CreateFromSymbol(x);
                var ifDirective = x.GetDefinedGenerateIfCondition();
                var hasIgnore = HasIgnoreAttribute(x);
                if (hasIgnore)
                {
                    return null;
                }

                var methods = x.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Select(symbol =>
                    {
                        if (HasIgnoreAttribute(symbol))
                        {
                            return null;
                        }
                        return CreateHubMethodInfoFromMethodSymbol(serviceType, symbol);
                    })
                    .Where(x => x is not null)
                    .Cast<MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo>()
                    .ToArray();

                var receiverInterfaceSymbol = x.AllInterfaces.First(y => y.ConstructedFrom.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHub)).TypeArguments[1];
                var receiverType = MagicOnionTypeInfo.CreateFromSymbol(receiverInterfaceSymbol);
                var receiverMethods = receiverInterfaceSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Select(y => CreateHubReceiverMethodInfoFromMethodSymbol(serviceType, receiverType, y))
                    .ToArray();

                var receiver = new MagicOnionStreamingHubInfo.MagicOnionStreamingHubReceiverInfo(receiverType, receiverMethods, receiverInterfaceSymbol.GetDefinedGenerateIfCondition());

                return new MagicOnionStreamingHubInfo(serviceType, methods, receiver, ifDirective);
            })
            .Where(x => x is not null)
            .Cast<MagicOnionStreamingHubInfo>()
            .OrderBy(x => x.ServiceType.FullName)
            .ToArray();
    }

    static int GetHubMethodIdFromMethodSymbol(IMethodSymbol methodSymbol)
        => (int?)methodSymbol.GetAttributes().FindAttributeShortName("MethodIdAttribute")?.ConstructorArguments[0].Value ?? FNV1A32.GetHashCode(methodSymbol.Name);
    static bool HasIgnoreAttribute(ISymbol symbol)
        => symbol.GetAttributes().FindAttributeShortName("IgnoreAttribute") is not null;

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
            case "global::System.Threading.Tasks.ValueTask":
                //responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
                break;
            case "global::System.Threading.Tasks.Task<>":
            case "global::System.Threading.Tasks.ValueTask<>":
                responseType = methodReturnType.GenericArguments[0];
                break;
            default:
                throw new InvalidOperationException($"StreamingHub method '{interfaceType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}' has unsupported return type '{methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified)}'.");
        }

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
    MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo CreateHubReceiverMethodInfoFromMethodSymbol(MagicOnionTypeInfo interfaceType, MagicOnionTypeInfo receiverType, IMethodSymbol methodSymbol)
    {
        var hubId = GetHubMethodIdFromMethodSymbol(methodSymbol);
        var methodReturnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
        var methodParameters = CreateParameterInfoListFromMethodSymbol(methodSymbol);
        var ifDirective = methodSymbol.GetDefinedGenerateIfCondition();
        var requestType = CreateRequestTypeFromMethodParameters(methodParameters);
        var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
        if (methodReturnType != MagicOnionTypeInfo.KnownTypes.System_Void)
        {
            throw new InvalidOperationException($"StreamingHub receiver method '{receiverType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}' has unsupported return type '{methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}'.");
        }

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
                var hasIgnore = HasIgnoreAttribute(x);
                if (hasIgnore)
                {
                    return null;
                }

                var methods = x.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Select(symbol =>
                    {
                        if (HasIgnoreAttribute(symbol))
                        {
                            return null;
                        }
                        return CreateServiceMethodInfoFromMethodSymbol(serviceType, symbol);
                    })
                    .Where(x => x is not null)
                    .Cast<MagicOnionServiceInfo.MagicOnionServiceMethodInfo>()
                    .ToArray();

                return new MagicOnionServiceInfo(serviceType, methods, ifDirective);
            })
            .Where(x => x is not null)
            .Cast<MagicOnionServiceInfo>()
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
            case "global::MagicOnion.UnaryResult":
                methodType = MethodType.Unary;
                responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
                break;
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
        if (methodType == MethodType.Unary && responseType.Namespace == "MagicOnion" && (responseType.Name is "ClientStreamingResult" or "ServerStreamingResult" or "DuplexStreamingResult"))
        {
            throw new InvalidOperationException($"Unary methods can not return '{responseType.FullName}' ({serviceType.FullName}.{methodSymbol.Name})");
        }
        if ((methodType == MethodType.ClientStreaming || methodType == MethodType.DuplexStreaming) && methodParameters.Any())
        {
            throw new InvalidOperationException($"ClientStreaming and DuplexStreaming must have no parameters. ({serviceType.FullName}.{methodSymbol.Name})");
        }

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

        public MethodCollectorContext(ReferenceSymbols referenceSymbols, IReadOnlyList<INamedTypeSymbol> serviceAndHubInterfaces)
        {
            ReferenceSymbols = referenceSymbols;
            ServiceAndHubInterfaces = serviceAndHubInterfaces;

            var serviceInterfaces = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var hubInterfaces = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
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

        public static MethodCollectorContext CreateFromCompilation(Compilation compilation, ReferenceSymbols referenceSymbols)
        {
            var serviceAndHubInterfaces = compilation.GetNamedTypeSymbols()
                .Where(x => x.TypeKind == TypeKind.Interface)
                .Where(x =>
                {
                    var all = x.AllInterfaces;
                    if (all.Any(y => y.ApproximatelyEqual(referenceSymbols.IServiceMarker)) || all.Any(y => y.ApproximatelyEqual(referenceSymbols.IStreamingHubMarker)))
                    {
                        return true;
                    }
                    return false;
                })
                .ToArray();

            return new MethodCollectorContext(referenceSymbols, serviceAndHubInterfaces);
        }

        public static MethodCollectorContext CreateFromInterfaceSymbols(ImmutableArray<INamedTypeSymbol> interfaceSymbols, ReferenceSymbols referenceSymbols)
        {
            var serviceAndHubInterfaces = interfaceSymbols
                .Where(x => x.TypeKind == TypeKind.Interface)
                .Where(x =>
                {
                    var all = x.AllInterfaces;
                    if (all.Any(y => y.ApproximatelyEqual(referenceSymbols.IServiceMarker)) || all.Any(y => y.ApproximatelyEqual(referenceSymbols.IStreamingHubMarker)))
                    {
                        return true;
                    }
                    return false;
                })
                .ToArray();

            return new MethodCollectorContext(referenceSymbols, serviceAndHubInterfaces);
        }
    }
}
