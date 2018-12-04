using MagicOnion.Utils;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicOnion.CodeAnalysis
{
    public class ReferenceSymbols
    {
        public static ReferenceSymbols Global;

        public readonly INamedTypeSymbol Void;
        public readonly INamedTypeSymbol Task;
        public readonly INamedTypeSymbol TaskOfT;
        public readonly INamedTypeSymbol UnaryResult;
        public readonly INamedTypeSymbol ClientStreamingResult;
        public readonly INamedTypeSymbol ServerStreamingResult;
        public readonly INamedTypeSymbol DuplexStreamingResult;
        public readonly INamedTypeSymbol GenerateDefineIf;
        public readonly INamedTypeSymbol IServiceMarker;
        public readonly INamedTypeSymbol IStreamingHubMarker;
        public readonly INamedTypeSymbol IStreamingHub;
        public readonly INamedTypeSymbol MethodIdAttribute;

        public ReferenceSymbols(Compilation compilation)
        {
            Void = compilation.GetTypeByMetadataName("System.Void");
            TaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            Task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            UnaryResult = compilation.GetTypeByMetadataName("MagicOnion.UnaryResult`1");
            ClientStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.ClientStreamingResult`2");
            DuplexStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.DuplexStreamingResult`2");
            ServerStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.ServerStreamingResult`1");
            GenerateDefineIf = compilation.GetTypeByMetadataName("MagicOnion.GenerateDefineIfAttribute");
            IStreamingHubMarker = compilation.GetTypeByMetadataName("MagicOnion.IStreamingHubMarker");
            IServiceMarker = compilation.GetTypeByMetadataName("MagicOnion.IServiceMarker");
            IStreamingHub = compilation.GetTypeByMetadataName("MagicOnion.IStreamingHub`2");
            MethodIdAttribute = compilation.GetTypeByMetadataName("MagicOnion.Server.Hubs.MethodIdAttribute");

            Global = this;
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

        public MethodCollector(string csProjPath, IEnumerable<string> conditinalSymbols)
        {
            this.csProjPath = csProjPath;
            var compilation = RoslynExtensions.GetCompilationFromProject(csProjPath, conditinalSymbols.ToArray()).GetAwaiter().GetResult();
            this.typeReferences = new ReferenceSymbols(compilation);

            var bothInterfaces = compilation.GetNamedTypeSymbols()
                .Where(x => x.TypeKind == TypeKind.Interface)
                .Where(x =>
                {
                    var all = x.AllInterfaces;
                    if (all.Any(y => y == typeReferences.IServiceMarker) || all.Any(y => y == typeReferences.IStreamingHubMarker))
                    {
                        return true;
                    }
                    return false;
                })
                .ToArray();

            serviceInterfaces = bothInterfaces
                .Where(x => x.AllInterfaces.Any(y => y == typeReferences.IServiceMarker))
                .Distinct()
                .ToArray();

            hubInterfaces = bothInterfaces
                .Where(x => x.AllInterfaces.Any(y => y == typeReferences.IStreamingHubMarker))
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
                    IsServiceDifinition = true,
                    IsIfDebug = x.GetAttributes().FindAttributeShortName("GenerateDefineDebugAttribute") != null,
                    Methods = x.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Select(CreateMethodDefinition)
                        .ToArray()
                })
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

                    var receiver = x.AllInterfaces.First(y => y.ConstructedFrom == this.typeReferences.IStreamingHub).TypeArguments[1];

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

            return new MethodDefinition
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
            var retType = method.ReturnType as INamedTypeSymbol;

            var constructedFrom = retType.ConstructedFrom;
            if (constructedFrom == typeReferences.TaskOfT)
            {
                retType = retType.TypeArguments[0] as INamedTypeSymbol;
                constructedFrom = retType.ConstructedFrom;
            }

            if (constructedFrom == typeReferences.UnaryResult)
            {
                methodType = MethodType.Unary;
                requestType = (method.Parameters.Length == 1) ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
                responseType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[0];
            }
            else if (constructedFrom == typeReferences.ServerStreamingResult)
            {
                methodType = MethodType.ServerStreaming;
                requestType = (method.Parameters.Length == 1) ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
                responseType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[0];
            }
            else if (constructedFrom == typeReferences.ClientStreamingResult)
            {
                methodType = MethodType.ClientStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[1];
            }
            else if (constructedFrom == typeReferences.DuplexStreamingResult)
            {
                methodType = MethodType.DuplexStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                unwrappedOriginalResponseType = retType.TypeArguments[1];
            }
            else
            {
                // no validation
                methodType = MethodType.Other;
                requestType = null;
                responseType = null;
                unwrappedOriginalResponseType = (retType.TypeArguments.Length != 0) ? retType.TypeArguments[0] : retType;
            }
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
