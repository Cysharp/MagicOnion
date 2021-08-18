using MagicOnion.GeneratorCore.Utils;
using MagicOnion.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MagicOnion.CodeAnalysis
{
    public class ReferenceSymbols
    {
        public readonly INamedTypeSymbol Void;
        public readonly INamedTypeSymbol Task;
        public readonly INamedTypeSymbol TaskOfT;
        public readonly INamedTypeSymbol UnaryResult;
        public readonly INamedTypeSymbol ClientStreamingResult;
        public readonly INamedTypeSymbol ServerStreamingResult;
        public readonly INamedTypeSymbol DuplexStreamingResult;
        public readonly INamedTypeSymbol IServiceMarker;
        public readonly INamedTypeSymbol IService;
        public readonly INamedTypeSymbol IStreamingHubMarker;
        public readonly INamedTypeSymbol IStreamingHub;
        public readonly INamedTypeSymbol MethodIdAttribute;

        public ReferenceSymbols(Compilation compilation, Action<string> logger)
        {
            Void = compilation.GetTypeByMetadataName("System.Void");
            if (Void == null)
            {
                logger("failed to get metadata of System.Void.");
            }

            TaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            if (TaskOfT == null)
            {
                logger("failed to get metadata of System.Threading.Tasks.Task`1.");
            }

            Task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            if (Task == null)
            {
                logger("failed to get metadata of System.Threading.Tasks.Task.");
            }

            INamedTypeSymbol GetTypeSymbolOrThrow(string name)
            {
                var symbol = compilation.GetTypeByMetadataName(name);
                if (symbol == null)
                {
                    throw new InvalidOperationException("failed to get metadata of " + name);
                }
                return symbol;
            }

            UnaryResult = GetTypeSymbolOrThrow("MagicOnion.UnaryResult`1");
            ClientStreamingResult = GetTypeSymbolOrThrow("MagicOnion.ClientStreamingResult`2");
            DuplexStreamingResult = GetTypeSymbolOrThrow("MagicOnion.DuplexStreamingResult`2");
            ServerStreamingResult = GetTypeSymbolOrThrow("MagicOnion.ServerStreamingResult`1");
            IStreamingHubMarker = GetTypeSymbolOrThrow("MagicOnion.IStreamingHubMarker");
            IServiceMarker = GetTypeSymbolOrThrow("MagicOnion.IServiceMarker");
            IStreamingHub = GetTypeSymbolOrThrow("MagicOnion.IStreamingHub`2");
            IService = GetTypeSymbolOrThrow("MagicOnion.IService`1");
            MethodIdAttribute = GetTypeSymbolOrThrow("MagicOnion.Server.Hubs.MethodIdAttribute");
        }
    }

    public class MethodCollector
    {
        static readonly SymbolDisplayFormat binaryWriteFormat = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);

        static readonly SymbolDisplayFormat shortTypeNameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

        readonly string csProjPath;
        readonly INamedTypeSymbol[] serviceInterfaces;
        readonly INamedTypeSymbol[] hubInterfaces;
        readonly ReferenceSymbols typeReferences;

        public MethodCollector(string csProjPath, IEnumerable<string> conditinalSymbols, Action<string> logger)
        {
            this.csProjPath = csProjPath;
            var compilation = PseudoCompilation.CreateFromProjectAsync(new[] { csProjPath }, conditinalSymbols.ToArray(), CancellationToken.None).GetAwaiter().GetResult();
            this.typeReferences = new ReferenceSymbols(compilation, logger);

            var bothInterfaces = compilation.GetNamedTypeSymbols()
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

            serviceInterfaces = bothInterfaces
                .Where(x => x.AllInterfaces.Any(y => y.ApproximatelyEqual(typeReferences.IServiceMarker)) && x.AllInterfaces.All(y => !y.ApproximatelyEqual(typeReferences.IStreamingHubMarker)))
                .Where(x => !x.ConstructedFrom.ApproximatelyEqual(this.typeReferences.IService))
                .Distinct()
                .ToArray();

            hubInterfaces = bothInterfaces
                .Where(x => x.AllInterfaces.Any(y => y.ApproximatelyEqual(typeReferences.IStreamingHubMarker)))
                .Where(x => !x.ConstructedFrom.ApproximatelyEqual(this.typeReferences.IStreamingHub))
                .Distinct()
                .ToArray();
        }

        public InterfaceDefinition[] CollectServiceInterface()
        {
            return serviceInterfaces
                .Select(x => new InterfaceDefinition()
                {
                    Name = x.ToDisplayString(shortTypeNameFormat),
                    FullName = x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Namespace = x.ContainingNamespace.IsGlobalNamespace ? null : x.ContainingNamespace.ToDisplayString(),
                    IsServiceDefinition = true,
                    IsIfDebug = x.GetAttributes().FindAttributeShortName("GenerateDefineDebugAttribute") != null,
                    Methods = x.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(CreateMethodDefinition)
                        .ToArray()
                })
                .OrderBy(x => x.FullName)
                .ToArray();
        }

        public (InterfaceDefinition hubDefinition, InterfaceDefinition receiverDefintion)[] CollectHubInterface()
        {
            return hubInterfaces
                .Select(x =>
                {
                    var hubDefinition = new InterfaceDefinition()
                    {
                        Name = x.ToDisplayString(shortTypeNameFormat),
                        FullName = x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Namespace = x.ContainingNamespace.IsGlobalNamespace ? null : x.ContainingNamespace.ToDisplayString(),
                        IsIfDebug = x.GetAttributes().FindAttributeShortName("GenerateDefineDebugAttribute") != null,
                        Methods = x.GetMembers()
                            .OfType<IMethodSymbol>()
                            .Select(CreateMethodDefinition)
                            .ToArray()
                    };

                    var receiver = x.AllInterfaces.First(y => y.ConstructedFrom.ApproximatelyEqual(this.typeReferences.IStreamingHub)).TypeArguments[1];

                    var receiverDefinition = new InterfaceDefinition()
                    {
                        Name = receiver.ToDisplayString(shortTypeNameFormat),
                        FullName = receiver.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Namespace = receiver.ContainingNamespace.IsGlobalNamespace ? null : receiver.ContainingNamespace.ToDisplayString(),
                        IsIfDebug = receiver.GetAttributes().FindAttributeShortName("GenerateDefineDebugAttribute") != null,
                        Methods = receiver.GetMembers()
                            .OfType<IMethodSymbol>()
                            .Select(CreateMethodDefinition)
                            .ToArray()
                    };

                    return (hubDefinition, receiverDefinition);
                })
                .OrderBy(x => x.hubDefinition.FullName)
                .ThenBy(x => x.receiverDefinition.FullName)
                .ToArray();
        }

        private class MethodNameComparer : IEqualityComparer<IMethodSymbol>
        {
            public static IEqualityComparer<IMethodSymbol> Instance { get; } = new MethodNameComparer();
            public bool Equals(IMethodSymbol x, IMethodSymbol y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(IMethodSymbol obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private MethodDefinition CreateMethodDefinition(IMethodSymbol y)
        {
            MethodType t;
            string requestType;
            string responseType;
            ITypeSymbol unwrappedOriginalResponseType;
            ExtractRequestResponseType(y, out t, out requestType, out responseType, out unwrappedOriginalResponseType);

            var id = FNV1A32.GetHashCode(y.Name);
            var idAttr = y.GetAttributes().FindAttributeShortName("MethodIdAttribute");
            if (idAttr != null)
            {
                id = (int)idAttr.ConstructorArguments[0].Value;
            }

            return new MethodDefinition(typeReferences)
            {
                Name = y.Name,
                MethodType = t,
                RequestType = requestType,
                ResponseType = responseType,
                UnwrappedOriginalResposneTypeSymbol = unwrappedOriginalResponseType,
                OriginalResponseTypeSymbol = y.ReturnType,
                IsIfDebug = y.GetAttributes().FindAttributeShortName("GenerateDefineDebugAttribute") != null,
                HubId = id,
                Parameters = y.Parameters.Select(p =>
                {
                    return new ParameterDefinition
                    {
                        ParameterName = p.Name,
                        TypeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        HasDefaultValue = p.HasExplicitDefaultValue,
                        DefaultValue = GetDefaultValue(p),
                        OriginalSymbol = p
                    };
                }).ToArray()
            };
        }

        void ExtractRequestResponseType(IMethodSymbol method, out MethodType methodType, out string requestType, out string responseType, out ITypeSymbol unwrappedOriginalResponseType)
        {
            // Acceptable ReturnTypes:
            //   - Void
            //   - UnaryResult<T>
            //   - Task<ServerStreamingResult<TResponse>>
            //   - Task<ClientStreamingResult<TRequest, TResponse>>
            //   - Task<DuplexStreamingResult<TRequest, TResponse>> 
            //   - Task (for StreamingHub)
            //   - Task<T> (for StreamingHub)
            var retType = method.ReturnType as INamedTypeSymbol;
            var isTaskResponse = false;
            ITypeSymbol retType2 = null;
            if (retType == null)
            {
                goto EMPTY;
            }

            if (!retType.IsGenericType && !retType.ApproximatelyEqual(typeReferences.Task) && !retType.ApproximatelyEqual(typeReferences.Void))
            {
                throw new InvalidOperationException($"A return type of a method must be 'void', 'UnaryResult<T>', 'Task<T>', 'Task'. (Method: {method.ToDisplayString()}, Return: {retType.ToDisplayString()})");
            }

            var constructedFrom = retType.ConstructedFrom;

            // Task<T>
            if (constructedFrom.ApproximatelyEqual(typeReferences.TaskOfT))
            {
                isTaskResponse = true;
                retType2 = retType.TypeArguments[0];
                retType = retType2 as INamedTypeSymbol;
                constructedFrom = retType?.ConstructedFrom;

                if (constructedFrom.ApproximatelyEqual(typeReferences.UnaryResult) ||
                    constructedFrom.ApproximatelyEqual(typeReferences.ServerStreamingResult) ||
                    constructedFrom.ApproximatelyEqual(typeReferences.ClientStreamingResult) ||
                    constructedFrom.ApproximatelyEqual(typeReferences.DuplexStreamingResult))
                {
                    // Unwrap T (No-op)
                }
                else
                {
                    // Task<T> (for StreamingHub method)
                    methodType = MethodType.Other;
                    requestType = null;
                    responseType = null;
                    unwrappedOriginalResponseType = retType2;
                    return;
                }
            }

            // UnaryResult<T>
            if (constructedFrom.ApproximatelyEqual(typeReferences.UnaryResult))
            {
                methodType = MethodType.Unary;
                requestType = (method.Parameters.Length == 1) ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
                responseType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[0];

                // Cannot allow to use StreamingResult as a type parameter of UnaryResult.
                if (retType.TypeArguments[0] is INamedTypeSymbol retTypeTypeArg0 &&
                    (
                        retTypeTypeArg0.ConstructedFrom.ApproximatelyEqual(typeReferences.ServerStreamingResult) ||
                        retTypeTypeArg0.ConstructedFrom.ApproximatelyEqual(typeReferences.ClientStreamingResult) ||
                        retTypeTypeArg0.ConstructedFrom.ApproximatelyEqual(typeReferences.DuplexStreamingResult)
                    )
                )
                {
                    throw new InvalidOperationException($"Cannot allow to use StreamingResult as a type parameter of UnaryResult. (Method: {method.ToDisplayString()}, Return: {retType.ToDisplayString()})");
                }
                return;
            }

            // ServerStreamingResult<TResponse>
            // ClientStreamingResult<TRequest, TResponse>
            // DuplexStreamingResult<TRequest, TResponse>
            if (constructedFrom.ApproximatelyEqual(typeReferences.ServerStreamingResult))
            {
                if (!isTaskResponse)
                {
                    throw new InvalidOperationException($"CodeGenerator doesn't support generating non-asynchronous StreamingResult call. (Method: {method.ToDisplayString()}, Return: {retType.ToDisplayString()})");
                }

                methodType = MethodType.ServerStreaming;
                requestType = (method.Parameters.Length == 1) ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
                responseType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[0];
                return;
            }
            else if (constructedFrom.ApproximatelyEqual(typeReferences.ClientStreamingResult))
            {
                if (!isTaskResponse)
                {
                    throw new InvalidOperationException($"CodeGenerator doesn't support generating non-asynchronous StreamingResult call. (Method: {method.ToDisplayString()}, Return: {retType.ToDisplayString()})");
                }

                methodType = MethodType.ClientStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[1];
                return;
            }
            else if (constructedFrom.ApproximatelyEqual(typeReferences.DuplexStreamingResult))
            {
                if (!isTaskResponse)
                {
                    throw new InvalidOperationException($"CodeGenerator doesn't support generating non-asynchronous StreamingResult call. (Method: {method.ToDisplayString()}, Return: {retType.ToDisplayString()})");
                }

                methodType = MethodType.DuplexStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[1];
                return;
            }

            EMPTY:
            // no validation
            methodType = MethodType.Other;
            requestType = null;
            responseType = null;
            unwrappedOriginalResponseType = (retType == null) ? retType2 : (retType.TypeArguments.Length != 0) ? retType.TypeArguments[0] : retType;
        }

        string GetDefaultValue(IParameterSymbol p)
        {
            if (p.HasExplicitDefaultValue)
            {
                var ppp = p.ToDisplayParts(new SymbolDisplayFormat(parameterOptions: SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue));

                if (!ppp.Any(x => x.Kind == SymbolDisplayPartKind.Keyword && x.ToString() == "default"))
                {
                    var l = ppp.Last();
                    if (l.Kind == SymbolDisplayPartKind.FieldName)
                    {
                        return l.Symbol.ToDisplayString();
                    }
                    else
                    {
                        return l.ToString();
                    }
                }
            }

            return "default(" + p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")";
        }
    }
}

namespace MagicOnion.Utils
{
    public static class FNV1A32
    {
        public static int GetHashCode(string str)
        {
            return GetHashCode(Encoding.UTF8.GetBytes(str));
        }

        public static int GetHashCode(byte[] obj)
        {
            uint hash = 0;
            if (obj != null)
            {
                hash = 2166136261;
                for (int i = 0; i < obj.Length; i++)
                {
                    hash = unchecked((obj[i] ^ hash) * 16777619);
                }
            }

            return unchecked((int)hash);
        }
    }
}
