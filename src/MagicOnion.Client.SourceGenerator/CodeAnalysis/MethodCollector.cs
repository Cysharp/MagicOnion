using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MagicOnion.Client.SourceGenerator.Internal;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

/// <summary>
/// Provides logic to collect MagicOnion Services and StreamingHubs from a compilation.
/// </summary>
public static class MethodCollector
{
    public static (MagicOnionServiceCollection ServiceCollection, IReadOnlyList<Diagnostic> Diagnostics) Collect(ImmutableArray<INamedTypeSymbol> interfaceSymbols, ReferenceSymbols referenceSymbols, CancellationToken cancellationToken)
    {
        var ctx = MethodCollectorContext.CreateFromInterfaceSymbols(interfaceSymbols, referenceSymbols, cancellationToken);

        return (new MagicOnionServiceCollection(GetStreamingHubs(ctx, cancellationToken), GetServices(ctx, cancellationToken)), ctx.ReportDiagnostics);
    }

    static IReadOnlyList<MagicOnionStreamingHubInfo> GetStreamingHubs(MethodCollectorContext ctx, CancellationToken cancellationToken)
    {
        return ctx.HubInterfaces
            .Select(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serviceType = ctx.GetOrCreateTypeInfoFromSymbol(x);
                var hasIgnore = HasIgnoreAttribute(x);
                if (hasIgnore)
                {
                    return null;
                }

                var hasError = false;
                var methods = new List<MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo>();
                foreach (var methodSymbol in x.GetMembers().OfType<IMethodSymbol>())
                {
                    if (HasIgnoreAttribute(methodSymbol)) continue;
                    if (TryCreateHubMethodInfoFromMethodSymbol(ctx, serviceType, methodSymbol, out var methodInfo, out var diagnostic))
                    {
                        methods.Add(methodInfo);
                    }

                    if (diagnostic is not null)
                    {
                        ctx.ReportDiagnostics.Add(diagnostic);
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            hasError = true;
                        }
                    }
                }

                var receiverInterfaceSymbol = x.AllInterfaces.First(y => y.ConstructedFrom.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHub)).TypeArguments[1];
                var receiverType = ctx.GetOrCreateTypeInfoFromSymbol(receiverInterfaceSymbol);

                var receiverMethods = new List<MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo>();
                foreach (var methodSymbol in receiverInterfaceSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    if (TryCreateHubReceiverMethodInfoFromMethodSymbol(ctx, serviceType, receiverType, methodSymbol, out var methodInfo, out var diagnostic))
                    {
                        receiverMethods.Add(methodInfo);
                    }

                    if (diagnostic is not null)
                    {
                        ctx.ReportDiagnostics.Add(diagnostic);
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            hasError = true;
                        }
                    }
                }

                if (hasError)
                {
                    return null;
                }

                var receiver = new MagicOnionStreamingHubInfo.MagicOnionStreamingHubReceiverInfo(receiverType, receiverMethods);

                return new MagicOnionStreamingHubInfo(serviceType, methods, receiver);
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

    static bool TryCreateHubMethodInfoFromMethodSymbol(MethodCollectorContext ctx, MagicOnionTypeInfo interfaceType, IMethodSymbol methodSymbol, [NotNullWhen(true)] out MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo? methodInfo, out Diagnostic? diagnostic)
    {
        var hubId = GetHubMethodIdFromMethodSymbol(methodSymbol);
        var methodReturnType = ctx.GetOrCreateTypeInfoFromSymbol(methodSymbol.ReturnType);
        var methodParameters = CreateParameterInfoListFromMethodSymbol(ctx, methodSymbol);
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
                methodInfo = null;
                diagnostic = Diagnostic.Create(
                    MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType,
                    methodSymbol.Locations.FirstOrDefault(), null, null,
                    $"{interfaceType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}", methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.FullyQualified));
                return false;
        }

        methodInfo = new MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo(
            hubId,
            methodSymbol.Name,
            methodParameters,
            methodReturnType,
            requestType,
            responseType
        );
        diagnostic = null;
        return true;
    }
    static bool TryCreateHubReceiverMethodInfoFromMethodSymbol(MethodCollectorContext ctx, MagicOnionTypeInfo interfaceType, MagicOnionTypeInfo receiverType, IMethodSymbol methodSymbol, [NotNullWhen(true)] out MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo? methodInfo, out Diagnostic? diagnostic)
    {
        var hubId = GetHubMethodIdFromMethodSymbol(methodSymbol);
        var methodReturnType = ctx.GetOrCreateTypeInfoFromSymbol(methodSymbol.ReturnType);
        var methodParameters = CreateParameterInfoListFromMethodSymbol(ctx, methodSymbol);
        var requestType = CreateRequestTypeFromMethodParameters(methodParameters);
        var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
        if (methodReturnType != MagicOnionTypeInfo.KnownTypes.System_Void)
        {
            methodInfo = null;
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedReceiverMethodReturnType,
                methodSymbol.Locations.FirstOrDefault(), null, null,
                $"{receiverType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace)}.{methodSymbol.Name}", methodReturnType.ToDisplayName(MagicOnionTypeInfo.DisplayNameFormat.Namespace));
            return false;
        }

        methodInfo = new MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo(
            hubId,
            methodSymbol.Name,
            methodParameters,
            methodReturnType,
            requestType,
            responseType
        );
        diagnostic = null;
        return true;
    }
    static IReadOnlyList<MagicOnionServiceInfo> GetServices(MethodCollectorContext ctx, CancellationToken cancellationToken)
    {
        return ctx.ServiceInterfaces
            .Select(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serviceType = ctx.GetOrCreateTypeInfoFromSymbol(x);
                var hasIgnore = HasIgnoreAttribute(x);
                if (hasIgnore)
                {
                    return null;
                }

                var methods = new List<MagicOnionServiceInfo.MagicOnionServiceMethodInfo>();
                var hasError = false;
                foreach (var methodSymbol in x.GetMembers().OfType<IMethodSymbol>())
                {
                    if (HasIgnoreAttribute(methodSymbol)) continue;
                    if (TryCreateServiceMethodInfoFromMethodSymbol(ctx, serviceType, methodSymbol, out var methodInfo, out var diagnostic))
                    {
                        methods.Add(methodInfo);
                    }

                    if (diagnostic is not null)
                    {
                        ctx.ReportDiagnostics.Add(diagnostic);
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            hasError = true;
                        }
                    }
                }

                if (hasError)
                {
                    return null;
                }

                return new MagicOnionServiceInfo(serviceType, methods);
            })
            .Where(x => x is not null)
            .Cast<MagicOnionServiceInfo>()
            .OrderBy(x => x.ServiceType.FullName)
            .ToArray();
    }

    static bool TryCreateServiceMethodInfoFromMethodSymbol(MethodCollectorContext ctx, MagicOnionTypeInfo serviceType, IMethodSymbol methodSymbol, [NotNullWhen(true)] out MagicOnionServiceInfo.MagicOnionServiceMethodInfo? serviceMethodInfo, out Diagnostic? diagnostic)
    {
        var methodReturnType = ctx.GetOrCreateTypeInfoFromSymbol(methodSymbol.ReturnType);
        var methodParameters = CreateParameterInfoListFromMethodSymbol(ctx, methodSymbol);
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
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType,
                methodSymbol.Locations.FirstOrDefault(), null, null,
                methodReturnType.FullName, $"{serviceType.FullName}.{methodSymbol.Name}");
            serviceMethodInfo = null;
            return false;
        }
        if (methodType == MethodType.Unary && responseType.Namespace == "MagicOnion" && (responseType.Name is "ClientStreamingResult" or "ServerStreamingResult" or "DuplexStreamingResult"))
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.UnaryUnsupportedMethodReturnType,
                methodSymbol.Locations.FirstOrDefault(), null, null,
                responseType.FullName, $"{serviceType.FullName}.{methodSymbol.Name}");
            serviceMethodInfo = null;
            return false;
        }
        if ((methodType == MethodType.ClientStreaming || methodType == MethodType.DuplexStreaming) && methodParameters.Any())
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.StreamingMethodMustHaveNoParameters,
                methodSymbol.Locations.FirstOrDefault(), null, null,
                $"{serviceType.FullName}.{methodSymbol.Name}");
            serviceMethodInfo = null;
            return false;
        }

        diagnostic = null;
        serviceMethodInfo = new MagicOnionServiceInfo.MagicOnionServiceMethodInfo(
            methodType,
            serviceType.Name,
            methodSymbol.Name,
            $"{serviceType.Name}/{methodSymbol.Name}",
            methodParameters,
            methodReturnType,
            requestType,
            responseType
        );
        return true;
    }

    static IReadOnlyList<MagicOnionMethodParameterInfo> CreateParameterInfoListFromMethodSymbol(MethodCollectorContext ctx, IMethodSymbol methodSymbol)
        => methodSymbol.Parameters.Select(x => MagicOnionMethodParameterInfo.CreateFromTypeInfoAndSymbol(ctx.GetOrCreateTypeInfoFromSymbol(x.Type), x)).ToArray();

    static MagicOnionTypeInfo CreateRequestTypeFromMethodParameters(IReadOnlyList<MagicOnionMethodParameterInfo> parameters)
        => (parameters.Count == 0)
            ? MagicOnionTypeInfo.KnownTypes.MessagePack_Nil
            : (parameters.Count == 1)
                ? parameters[0].Type
                : MagicOnionTypeInfo.CreateValueType("MagicOnion", "DynamicArgumentTuple", parameters.Select(x => x.Type).ToArray());

    record MethodCollectorContext
    {
        public required ReferenceSymbols ReferenceSymbols { get; init; }
        public required IReadOnlyList<INamedTypeSymbol> ServiceInterfaces { get; init; }
        public required IReadOnlyList<INamedTypeSymbol> HubInterfaces { get; init; }
        public List<Diagnostic> ReportDiagnostics { get; } = new();
        public Dictionary<ITypeSymbol, MagicOnionTypeInfo> TypeInfoCache { get; } = new (SymbolEqualityComparer.Default);

        public MagicOnionTypeInfo GetOrCreateTypeInfoFromSymbol(ITypeSymbol symbol)
        {
            if (TypeInfoCache.TryGetValue(symbol, out var typeInfo))
            {
                return typeInfo;
            }
            return TypeInfoCache[symbol] = MagicOnionTypeInfo.CreateFromSymbol(symbol);
        }

        public static MethodCollectorContext CreateFromInterfaceSymbols(ImmutableArray<INamedTypeSymbol> interfaceSymbols, ReferenceSymbols referenceSymbols, CancellationToken cancellationToken)
        {
            var serviceInterfaces = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var hubInterfaces = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var interfaceSymbol in interfaceSymbols)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var implInterfaceSymbol in interfaceSymbol.AllInterfaces)
                {
                    if (implInterfaceSymbol.ApproximatelyEqual(referenceSymbols.IStreamingHubMarker) &&
                        !interfaceSymbol.ConstructedFrom.ApproximatelyEqual(referenceSymbols.IStreamingHub))
                    {
                        // StreamingHub
                        hubInterfaces.Add(interfaceSymbol);
                    }
                    else if (implInterfaceSymbol.ApproximatelyEqual(referenceSymbols.IServiceMarker) &&
                             !interfaceSymbol.ConstructedFrom.ApproximatelyEqual(referenceSymbols.IService) &&
                             !hubInterfaces.Contains(interfaceSymbol) /* IStreamingHub also implements IService */)
                    {
                        // Service
                        serviceInterfaces.Add(interfaceSymbol);
                    }
                }
            }

            return new MethodCollectorContext
            {
                HubInterfaces = hubInterfaces.ToArray(),
                ServiceInterfaces = serviceInterfaces.ToArray(),
                ReferenceSymbols = referenceSymbols,
            };
        }
    }
}
