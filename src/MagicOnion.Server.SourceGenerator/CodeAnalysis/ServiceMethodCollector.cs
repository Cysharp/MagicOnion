using Microsoft.CodeAnalysis;
using MagicOnion.Server.SourceGenerator.Internal;

namespace MagicOnion.Server.SourceGenerator.CodeAnalysis;

/// <summary>
/// Collects service implementation information from type symbols.
/// </summary>
public static class ServiceMethodCollector
{
    public static (IReadOnlyList<ServiceImplementationInfo> Services, IReadOnlyList<Diagnostic> Diagnostics) Collect(
        IReadOnlyList<INamedTypeSymbol> implementationTypes,
        ServerReferenceSymbols referenceSymbols,
        CancellationToken cancellationToken)
    {
        var services = new List<ServiceImplementationInfo>();
        var diagnostics = new List<Diagnostic>();

        foreach (var implType in implementationTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (TryCollectServiceImplementation(implType, referenceSymbols, out var serviceInfo, out var serviceDiagnostics))
            {
                services.Add(serviceInfo);
            }
            diagnostics.AddRange(serviceDiagnostics);
        }

        return (services, diagnostics);
    }

    static bool TryCollectServiceImplementation(
        INamedTypeSymbol implType,
        ServerReferenceSymbols referenceSymbols,
        out ServiceImplementationInfo serviceInfo,
        out List<Diagnostic> diagnostics)
    {
        serviceInfo = null!;
        diagnostics = new List<Diagnostic>();

        // Validate: must be non-abstract class
        if (implType.IsAbstract)
        {
            diagnostics.Add(Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.ServiceTypeMustBeNonAbstract,
                implType.Locations.FirstOrDefault(),
                implType.ToDisplayString()));
            return false;
        }

        // Find IService<T> or IStreamingHub<T, TReceiver> interface
        var serviceInterface = implType.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IService));
        var hubInterface = implType.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IStreamingHub));

        if (serviceInterface is null && hubInterface is null)
        {
            diagnostics.Add(Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.InvalidServiceType,
                implType.Locations.FirstOrDefault(),
                implType.ToDisplayString()));
            return false;
        }

        var isStreamingHub = hubInterface is not null;
        var interfaceType = isStreamingHub ? hubInterface!.TypeArguments[0] : serviceInterface!.TypeArguments[0];
        var receiverType = isStreamingHub ? hubInterface!.TypeArguments[1] : null;

        var implementationType = MagicOnionTypeInfo.CreateFromSymbol(implType);
        var serviceInterfaceType = MagicOnionTypeInfo.CreateFromSymbol(interfaceType);
        var receiverInterfaceType = receiverType is not null ? MagicOnionTypeInfo.CreateFromSymbol(receiverType) : null;

        // Collect methods
        var serviceMethods = new List<ServiceMethodInfo>();
        var hubMethods = new List<StreamingHubMethodInfo>();

        if (isStreamingHub)
        {
            // For StreamingHub, collect hub methods from the interface
            CollectStreamingHubMethods(implType, (INamedTypeSymbol)interfaceType, referenceSymbols, hubMethods, diagnostics);
        }
        else
        {
            // For Service, collect service methods
            CollectServiceMethods(implType, (INamedTypeSymbol)interfaceType, referenceSymbols, serviceMethods, diagnostics);
        }

        serviceInfo = new ServiceImplementationInfo(
            implementationType,
            serviceInterfaceType,
            isStreamingHub,
            receiverInterfaceType,
            serviceMethods,
            hubMethods);

        return true;
    }

    static void CollectServiceMethods(
        INamedTypeSymbol implType,
        INamedTypeSymbol interfaceType,
        ServerReferenceSymbols referenceSymbols,
        List<ServiceMethodInfo> methods,
        List<Diagnostic> diagnostics)
    {
        var serviceName = interfaceType.Name;

        foreach (var member in interfaceType.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol) continue;
            if (methodSymbol.MethodKind != MethodKind.Ordinary) continue;
            if (HasIgnoreAttribute(methodSymbol, referenceSymbols)) continue;

            // Skip well-known methods
            if (IsWellKnownMethod(methodSymbol.Name)) continue;

            if (TryCreateServiceMethodInfo(serviceName, methodSymbol, referenceSymbols, out var methodInfo, out var diagnostic))
            {
                methods.Add(methodInfo);
            }

            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        }
    }

    static void CollectStreamingHubMethods(
        INamedTypeSymbol implType,
        INamedTypeSymbol interfaceType,
        ServerReferenceSymbols referenceSymbols,
        List<StreamingHubMethodInfo> methods,
        List<Diagnostic> diagnostics)
    {
        var serviceName = interfaceType.Name;

        foreach (var member in GetAllInterfaceMembers(interfaceType, referenceSymbols))
        {
            if (member is not IMethodSymbol methodSymbol) continue;
            if (methodSymbol.MethodKind != MethodKind.Ordinary) continue;
            if (HasIgnoreAttribute(methodSymbol, referenceSymbols)) continue;

            // Skip well-known methods
            if (IsWellKnownMethod(methodSymbol.Name)) continue;

            if (TryCreateStreamingHubMethodInfo(serviceName, methodSymbol, referenceSymbols, out var methodInfo, out var diagnostic))
            {
                // Avoid duplicates
                if (!methods.Any(x => x.MethodName == methodInfo.MethodName))
                {
                    methods.Add(methodInfo);
                }
            }

            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        }
    }

    static IEnumerable<ISymbol> GetAllInterfaceMembers(INamedTypeSymbol interfaceType, ServerReferenceSymbols referenceSymbols)
    {
        foreach (var member in interfaceType.GetMembers())
        {
            yield return member;
        }

        foreach (var baseInterface in interfaceType.AllInterfaces)
        {
            if (baseInterface.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IStreamingHub)) continue;
            if (baseInterface.OriginalDefinition.ApproximatelyEqual(referenceSymbols.IService)) continue;

            foreach (var member in baseInterface.GetMembers())
            {
                yield return member;
            }
        }
    }

    static bool TryCreateServiceMethodInfo(
        string serviceName,
        IMethodSymbol methodSymbol,
        ServerReferenceSymbols referenceSymbols,
        out ServiceMethodInfo methodInfo,
        out Diagnostic? diagnostic)
    {
        methodInfo = null!;
        diagnostic = null;

        var returnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
        var parameters = CreateParameterInfoList(methodSymbol);
        var methodType = MethodType.Other;
        var requestType = CreateRequestType(parameters);
        var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;

        // Determine method type from return type
        if (methodSymbol.ReturnType.ApproximatelyEqual(referenceSymbols.UnaryResult))
        {
            // UnaryResult (no return value)
            methodType = MethodType.Unary;
            responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
        }
        else if (methodSymbol.ReturnType is INamedTypeSymbol { IsGenericType: true } namedReturnType)
        {
            var returnTypeOpen = namedReturnType.OriginalDefinition;

            if (returnTypeOpen.ApproximatelyEqual(referenceSymbols.UnaryResultOfT))
            {
                // UnaryResult<T>
                methodType = MethodType.Unary;
                responseType = MagicOnionTypeInfo.CreateFromSymbol(namedReturnType.TypeArguments[0]);
            }
            else if (returnTypeOpen.ApproximatelyEqual(referenceSymbols.TaskOfT))
            {
                var innerType = namedReturnType.TypeArguments[0];
                if (innerType is INamedTypeSymbol { IsGenericType: true } innerNamedType)
                {
                    var innerTypeOpen = innerNamedType.OriginalDefinition;

                    if (referenceSymbols.ClientStreamingResult is not null &&
                        innerTypeOpen.ApproximatelyEqual(referenceSymbols.ClientStreamingResult))
                    {
                        // Task<ClientStreamingResult<TRequest, TResponse>>
                        methodType = MethodType.ClientStreaming;
                        requestType = MagicOnionTypeInfo.CreateFromSymbol(innerNamedType.TypeArguments[0]);
                        responseType = MagicOnionTypeInfo.CreateFromSymbol(innerNamedType.TypeArguments[1]);
                    }
                    else if (referenceSymbols.ServerStreamingResult is not null &&
                             innerTypeOpen.ApproximatelyEqual(referenceSymbols.ServerStreamingResult))
                    {
                        // Task<ServerStreamingResult<T>>
                        methodType = MethodType.ServerStreaming;
                        responseType = MagicOnionTypeInfo.CreateFromSymbol(innerNamedType.TypeArguments[0]);
                    }
                    else if (referenceSymbols.DuplexStreamingResult is not null &&
                             innerTypeOpen.ApproximatelyEqual(referenceSymbols.DuplexStreamingResult))
                    {
                        // Task<DuplexStreamingResult<TRequest, TResponse>>
                        methodType = MethodType.DuplexStreaming;
                        requestType = MagicOnionTypeInfo.CreateFromSymbol(innerNamedType.TypeArguments[0]);
                        responseType = MagicOnionTypeInfo.CreateFromSymbol(innerNamedType.TypeArguments[1]);
                    }
                }
            }
        }

        if (methodType == MethodType.Other)
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.ServiceUnsupportedMethodReturnType,
                methodSymbol.Locations.FirstOrDefault(),
                returnType.FullName,
                $"{serviceName}.{methodSymbol.Name}");
            return false;
        }

        // Validate: streaming methods must have no parameters
        if ((methodType == MethodType.ClientStreaming || methodType == MethodType.DuplexStreaming) && parameters.Count > 0)
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.StreamingMethodMustHaveNoParameters,
                methodSymbol.Locations.FirstOrDefault(),
                $"{serviceName}.{methodSymbol.Name}");
            return false;
        }

        methodInfo = new ServiceMethodInfo(
            methodType,
            serviceName,
            methodSymbol.Name,
            parameters,
            returnType,
            requestType,
            responseType);

        return true;
    }

    static bool TryCreateStreamingHubMethodInfo(
        string serviceName,
        IMethodSymbol methodSymbol,
        ServerReferenceSymbols referenceSymbols,
        out StreamingHubMethodInfo methodInfo,
        out Diagnostic? diagnostic)
    {
        methodInfo = null!;
        diagnostic = null;

        var returnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
        var parameters = CreateParameterInfoList(methodSymbol);
        var requestType = CreateRequestType(parameters);
        var responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;

        // Get method ID from attribute or calculate from name
        var methodId = GetMethodId(methodSymbol, referenceSymbols);

        // Determine response type from return type
        if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Void ||
            methodSymbol.ReturnType.ApproximatelyEqual(referenceSymbols.Task) ||
            methodSymbol.ReturnType.ApproximatelyEqual(referenceSymbols.ValueTask))
        {
            responseType = MagicOnionTypeInfo.KnownTypes.MessagePack_Nil;
        }
        else if (methodSymbol.ReturnType is INamedTypeSymbol { IsGenericType: true } namedReturnType)
        {
            var returnTypeOpen = namedReturnType.OriginalDefinition;

            if (returnTypeOpen.ApproximatelyEqual(referenceSymbols.TaskOfT) ||
                returnTypeOpen.ApproximatelyEqual(referenceSymbols.ValueTaskOfT))
            {
                responseType = MagicOnionTypeInfo.CreateFromSymbol(namedReturnType.TypeArguments[0]);
            }
            else
            {
                diagnostic = Diagnostic.Create(
                    MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType,
                    methodSymbol.Locations.FirstOrDefault(),
                    returnType.FullName,
                    $"{serviceName}.{methodSymbol.Name}");
                return false;
            }
        }
        else
        {
            diagnostic = Diagnostic.Create(
                MagicOnionDiagnosticDescriptors.StreamingHubUnsupportedMethodReturnType,
                methodSymbol.Locations.FirstOrDefault(),
                returnType.FullName,
                $"{serviceName}.{methodSymbol.Name}");
            return false;
        }

        methodInfo = new StreamingHubMethodInfo(
            methodId,
            methodSymbol.Name,
            parameters,
            returnType,
            requestType,
            responseType);

        return true;
    }

    static int GetMethodId(IMethodSymbol methodSymbol, ServerReferenceSymbols referenceSymbols)
    {
        if (referenceSymbols.MethodIdAttribute is not null)
        {
            var attr = methodSymbol.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass?.ApproximatelyEqual(referenceSymbols.MethodIdAttribute) == true);

            if (attr is not null && attr.ConstructorArguments.Length > 0)
            {
                return (int)attr.ConstructorArguments[0].Value!;
            }
        }

        return FNV1A32.GetHashCode(methodSymbol.Name);
    }

    static IReadOnlyList<MagicOnionMethodParameterInfo> CreateParameterInfoList(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters
            .Select(x => MagicOnionMethodParameterInfo.CreateFromTypeInfoAndSymbol(
                MagicOnionTypeInfo.CreateFromSymbol(x.Type), x))
            .ToArray();
    }

    static MagicOnionTypeInfo CreateRequestType(IReadOnlyList<MagicOnionMethodParameterInfo> parameters)
    {
        return parameters.Count switch
        {
            0 => MagicOnionTypeInfo.KnownTypes.MessagePack_Nil,
            1 => parameters[0].Type,
            _ => MagicOnionTypeInfo.CreateValueType("MagicOnion", "DynamicArgumentTuple",
                parameters.Select(x => x.Type).ToArray())
        };
    }

    static bool HasIgnoreAttribute(IMethodSymbol methodSymbol, ServerReferenceSymbols referenceSymbols)
    {
        if (referenceSymbols.IgnoreAttribute is null) return false;
        return methodSymbol.GetAttributes()
            .Any(x => x.AttributeClass?.ApproximatelyEqual(referenceSymbols.IgnoreAttribute) == true);
    }

    static bool IsWellKnownMethod(string methodName)
    {
        return methodName is "Equals" or "GetHashCode" or "GetType" or "ToString"
            or "WithOptions" or "WithHeaders" or "WithDeadline" or "WithCancellationToken" or "WithHost"
            or "FireAndForget" or "DisposeAsync" or "WaitForDisconnect";
    }
}
