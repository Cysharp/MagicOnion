using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MagicOnion.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MagicOnion.GeneratorCore.CodeAnalysis
{
    public class MethodCollector2
    {
        readonly Action<string> logger;

        public MethodCollector2(Action<string> logger = null)
        {
            this.logger = logger ?? (_ => {});
        }

        public MagicOnionServiceCollection Collect(Compilation compilation)
        {
            var ctx = MethodCollectorContext.CreateFromCompilation(compilation, logger);

            var serviceInterfaces = ctx.ServiceAndHubInterfaces
                .Where(x => x.AllInterfaces.Any(y => y.ApproximatelyEqual(ctx.ReferenceSymbols.IServiceMarker)) && x.AllInterfaces.All(y => !y.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHubMarker)))
                .Where(x => !x.ConstructedFrom.ApproximatelyEqual(ctx.ReferenceSymbols.IService))
                .Distinct()
                .ToArray();

            var hubInterfaces = ctx.ServiceAndHubInterfaces
                .Where(x => x.AllInterfaces.Any(y => y.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHubMarker)))
                .Where(x => !x.ConstructedFrom.ApproximatelyEqual(ctx.ReferenceSymbols.IStreamingHub))
                .Distinct()
                .ToArray();

            return new MagicOnionServiceCollection(GetStreamingHubs(ctx, hubInterfaces), GetServices(ctx, serviceInterfaces));
        }

        private IReadOnlyList<MagicOnionStreamingHubInfo> GetStreamingHubs(MethodCollectorContext ctx, IReadOnlyList<INamedTypeSymbol> serviceInterfaces)
        {
            return Array.Empty<MagicOnionStreamingHubInfo>();
        }

        private IReadOnlyList<MagicOnionServiceInfo> GetServices(MethodCollectorContext ctx, IReadOnlyList<INamedTypeSymbol> serviceInterfaces)
        {
            return serviceInterfaces
                .Select(x =>
                {
                    var serviceType = MagicOnionTypeInfo.CreateFromSymbol(x);
                    var methods = x.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(y => CreateServiceMethodInfoFromMethodSymbol(serviceType, y))
                        .ToArray();

                    return new MagicOnionServiceInfo(serviceType, methods, x.GetDefinedGenerateIfCondition());
                })
                .OrderBy(x => x.ServiceType.FullName)
                .ToArray();
        }
        
        private MagicOnionServiceInfo.MagicOnionServiceMethodInfo CreateServiceMethodInfoFromMethodSymbol(MagicOnionTypeInfo serviceType, IMethodSymbol methodSymbol)
        {
            var ifDirective = methodSymbol.GetDefinedGenerateIfCondition();
            var methodReturnType = MagicOnionTypeInfo.CreateFromSymbol(methodSymbol.ReturnType);
            var methodParameters = methodSymbol.Parameters.Select(y => MagicOnionTypeInfo.CreateFromSymbol(y.Type)).ToArray();
            var requestType = (methodSymbol.Parameters.Length == 0)
                ? MagicOnionTypeInfo.KnownTypes.MessagePack_Nil
                : (methodSymbol.Parameters.Length == 1)
                    ? methodParameters[0]
                    : MagicOnionTypeInfo.Create("MagicOnion", "DynamicArgumentTuple", methodParameters);
            var responseType = MagicOnionTypeInfo.KnownTypes.System_Void;
            var methodType = MethodType.Other;
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

        public class MethodCollectorContext
        {
            public ReferenceSymbols ReferenceSymbols { get; }
            public IReadOnlyList<INamedTypeSymbol> ServiceAndHubInterfaces { get; }
            public Action<string> Logger { get; }

            public MethodCollectorContext(ReferenceSymbols referenceSymbols, IReadOnlyList<INamedTypeSymbol> serviceAndHubInterfaces, Action<string> logger)
            {
                ReferenceSymbols = referenceSymbols;
                ServiceAndHubInterfaces = serviceAndHubInterfaces;
                Logger = logger ?? (_ => { });
            }

            public static MethodCollectorContext CreateFromCompilation(Compilation compilation, Action<string> logger = null)
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
