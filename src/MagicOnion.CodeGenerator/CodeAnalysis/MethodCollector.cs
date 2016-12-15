using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicOnion.CodeAnalysis
{
    public class ReferenceSymbols
    {
        public readonly INamedTypeSymbol Task;
        public readonly INamedTypeSymbol TaskOfT;
        public readonly INamedTypeSymbol UnaryResult;
        public readonly INamedTypeSymbol ClientStreamingResult;
        public readonly INamedTypeSymbol ServerStreamingResult;
        public readonly INamedTypeSymbol DuplexStreamingResult;

        public ReferenceSymbols(Compilation compilation)
        {
            TaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            Task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            UnaryResult = compilation.GetTypeByMetadataName("MagicOnion.UnaryResult`1");
            ClientStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.ClientStreamingResult`2");
            DuplexStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.DuplexStreamingResult`2");
            ServerStreamingResult = compilation.GetTypeByMetadataName("MagicOnion.ServerStreamingResult`1");
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
        readonly INamedTypeSymbol[] targetTypes;
        readonly ReferenceSymbols typeReferences;

        public MethodCollector(string csProjPath, IEnumerable<string> conditinalSymbols)
        {
            this.csProjPath = csProjPath;
            var compilation = RoslynExtensions.GetCompilationFromProject(csProjPath, conditinalSymbols.ToArray()).GetAwaiter().GetResult();
            this.typeReferences = new ReferenceSymbols(compilation);

            var marker = compilation.GetTypeByMetadataName("MagicOnion.__IServiceMarker");

            targetTypes = compilation.GetNamedTypeSymbols()
                .Where(t => t.AllInterfaces.Any(x => x == marker))
                .ToArray();
        }

        // not visitor pattern:)
        public InterfaceDefintion[] Visit()
        {
            return targetTypes.Select(x => new InterfaceDefintion
            {
                Name = x.ToDisplayString(shortTypeNameFormat),
                Namespace = x.ContainingNamespace.IsGlobalNamespace ? null : x.ContainingNamespace.ToDisplayString(),
                Methods = x.GetAllMembers()
                    .OfType<IMethodSymbol>()
                    .Select(y =>
                    {
                        MethodType t;
                        string requestType;
                        string responseType;
                        ExtractRequestResponseType(y, out t, out requestType, out responseType);
                        return new MethodDefinition
                        {
                            Name = y.Name,
                            MethodType = t,
                            RequestType = requestType,
                            ResponseType = responseType,
                            Parameters = y.Parameters.Select(p =>
                            {
                                return new ParameterDefinition
                                {
                                    ParameterName = p.Name,
                                    TypeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                    HasDefaultValue = p.HasExplicitDefaultValue,
                                    DefaultValue = GetDefaultValue(p)
                                };
                            }).ToArray()
                        };
                    })
                    .ToArray()
            })
            .ToArray();
        }

        void ExtractRequestResponseType(IMethodSymbol method, out MethodType methodType, out string requestType, out string responseType)
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
            }
            else if (constructedFrom == typeReferences.ServerStreamingResult)
            {
                methodType = MethodType.ServerStreaming;
                requestType = (method.Parameters.Length == 1) ? method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
                responseType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else if (constructedFrom == typeReferences.ClientStreamingResult)
            {
                methodType = MethodType.ClientStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else if (constructedFrom == typeReferences.DuplexStreamingResult)
            {
                methodType = MethodType.DuplexStreaming;
                requestType = retType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = retType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else
            {
                throw new Exception("Invalid Return Type, method:" + method.Name + " returnType:" + method.ReturnType);
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