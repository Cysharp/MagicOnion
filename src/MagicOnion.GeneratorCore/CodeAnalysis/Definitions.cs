using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicOnion.CodeAnalysis
{
    public enum MethodType
    {
        Unary = 0,
        ClientStreaming = 1,
        ServerStreaming = 2,
        DuplexStreaming = 3,
        Other = 99
    }

    public class InterfaceDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Namespace { get; set; }
        public bool IsServiceDefinition { get; set; }
        public bool IsIfDebug { get; set; }
        public MethodDefinition[] Methods { get; set; }

        // NOTE: A client name is derived from original interface name without 'I' prefix.
        // - ImportantService  --> ImportantServiceClient
        // - IImportantService --> ImportantServiceClient
        // - I0123Service      --> I0123ServiceClient
        public string ClientName => (Regex.IsMatch(Name, "I[^a-z0-9]") ? Name.Substring(1) : Name) + "Client";
        public string ClientFullName => (Namespace != null ? Namespace + "." : "") + ClientName;

        public override string ToString()
        {
            return (Namespace != null)
                ? Namespace + "." + Name
                : Name;
        }
    }

    public class MethodDefinition
    {
        readonly ReferenceSymbols referenceSymbols;

        public string Name { get; set; }
        public MethodType MethodType { get; set; }
        public string RequestType { get; set; }
        public bool IsIfDebug { get; set; }
        public int HubId { get; set; } // only use in Hub.

        string responseType;
        public string ResponseType
        {
            get
            {
                return responseType;
            }
            set
            {
                responseType = value;
            }
        }
        public ParameterDefinition[] Parameters { get; set; }

        public ITypeSymbol UnwrappedOriginalResposneTypeSymbol { get; set; }
        public ITypeSymbol OriginalResponseTypeSymbol { get; set; }


        public bool IsResponseTypeTaskOfT
        {
            get
            {
                return (OriginalResponseTypeSymbol as INamedTypeSymbol)?.ConstructedFrom.ApproximatelyEqual(referenceSymbols.TaskOfT) ?? false;
            }
        }

        public string ReturnType
        {
            get
            {
                // v2, returns original type.
                return OriginalResponseTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        public string RequestMarshallerType
        {
            get
            {
                switch (MethodType)
                {
                    case MethodType.Unary:
                    case MethodType.ServerStreaming:
                        if (Parameters.Length == 0)
                        {
                            return "Nil";
                        }
                        else if (Parameters.Length == 1)
                        {
                            return RequestType;
                        }
                        else
                        {
                            var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                            return $"DynamicArgumentTuple<{typeArgs}>";
                        }
                    case MethodType.ClientStreaming:
                    case MethodType.DuplexStreaming:
                    default:
                        return RequestType;
                }
            }
        }

        public string UnaryRequestObject
        {
            get
            {
                if (Parameters.Length == 0)
                {
                    return $"Nil.Default";
                }
                else if (Parameters.Length == 1)
                {
                    return Parameters[0].ParameterName;
                }
                else
                {
                    var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                    var parameterNames = string.Join(", ", Parameters.Select(x => x.ParameterName));
                    return $"new DynamicArgumentTuple<{typeArgs}>({parameterNames})";
                }
            }
        }

        public MethodDefinition(ReferenceSymbols referenceSymbols)
        {
            this.referenceSymbols = referenceSymbols;
        }

        public string RequestObject()
        {
            if (Parameters.Length == 0)
            {
                return $"MagicOnionMarshallers.UnsafeNilBytes";
            }
            else if (Parameters.Length == 1)
            {
                return $"MessagePackSerializer.Serialize({Parameters[0].ParameterName}, base.serializerOptions)";
            }
            else
            {
                var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                var parameterNames = string.Join(", ", Parameters.Select(x => x.ParameterName));
                return $"MessagePackSerializer.Serialize(new DynamicArgumentTuple<{typeArgs}>({parameterNames}), base.serializerOptions)";
            }
        }

        public string ToHubWriteMessage()
        {
            string parameterType;
            string requestObject;
            if (Parameters.Length == 0)
            {
                parameterType = "Nil";
                requestObject = "Nil.Default";
            }
            else if (Parameters.Length == 1)
            {
                parameterType = Parameters[0].TypeName;
                requestObject = Parameters[0].ParameterName;
            }
            else
            {
                var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                var parameterNames = string.Join(", ", Parameters.Select(x => x.ParameterName));

                parameterType = $"DynamicArgumentTuple<{typeArgs}>";
                requestObject = $"new DynamicArgumentTuple<{typeArgs}>({parameterNames})";
            }

            if (OriginalResponseTypeSymbol.ApproximatelyEqual(referenceSymbols.Task))
            {
                return $"WriteMessageWithResponseAsync<{parameterType}, Nil>({HubId}, {requestObject})";
            }
            else
            {
                return $"WriteMessageWithResponseAsync<{parameterType}, {UnwrappedOriginalResposneTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}> ({HubId}, {requestObject})";
            }
        }

        public string ToHubFireAndForgetWriteMessage()
        {
            string parameterType;
            string requestObject;
            if (Parameters.Length == 0)
            {
                parameterType = "Nil";
                requestObject = "Nil.Default";
            }
            else if (Parameters.Length == 1)
            {
                parameterType = Parameters[0].TypeName;
                requestObject = Parameters[0].ParameterName;
            }
            else
            {
                var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                var parameterNames = string.Join(", ", Parameters.Select(x => x.ParameterName));

                parameterType = $"DynamicArgumentTuple<{typeArgs}>";
                requestObject = $"new DynamicArgumentTuple<{typeArgs}>({parameterNames})";
            }

            if (OriginalResponseTypeSymbol.ApproximatelyEqual(referenceSymbols.Task))
            {
                return $"WriteMessageAsync<{parameterType}>({HubId}, {requestObject})";
            }
            else
            {
                return $"WriteMessageAsyncFireAndForget<{parameterType}, {UnwrappedOriginalResposneTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}> ({HubId}, {requestObject})";
            }
        }

        public (string line1, string line2) ToHubOnBroadcastMessage()
        {
            string parameterType;
            string line2;
            if (Parameters.Length == 0)
            {
                parameterType = "Nil";
                line2 = $"{Name}()";
            }
            else if (Parameters.Length == 1)
            {
                parameterType = Parameters[0].TypeName;
                line2 = $"{Name}(result)";
            }
            else
            {
                var typeArgs = string.Join(", ", Parameters.Select(x => x.TypeName));
                var parameterNames = string.Join(", ", Parameters.Select(x => x.ParameterName));

                parameterType = $"DynamicArgumentTuple<{typeArgs}>";
                line2 = string.Join(", ", Enumerable.Range(1, Parameters.Length).Select(x => $"result.Item{x}"));
                line2 = $"{Name}({line2})";
            }

            line2 = "receiver." + line2 + "; break;";

            var line1 = $"var result = MessagePackSerializer.Deserialize<{parameterType}>(data, serializerOptions);";
            return (line1, line2);
        }

        public (string line1, string line2) ToHubOnResponseEvent()
        {
            string type;
            if (OriginalResponseTypeSymbol.ApproximatelyEqual(referenceSymbols.Task))
            {
                type = "Nil";
            }
            else
            {
                type = UnwrappedOriginalResposneTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }

            var line1 = $"var result = MessagePackSerializer.Deserialize<{type}>(data, serializerOptions);";
            var line2 = $"((TaskCompletionSource<{type}>)taskCompletionSource).TrySetResult(result);";
            return (line1, line2);
        }

        public override string ToString()
        {
            return $"{ReturnType} {Name}({string.Join(", ", Parameters.Select(x => x.ToString()))})";
        }

        public string AsyncToString()
        {
            if (Name.EndsWith("Async"))
            {
                return ToString();
            }
            else
            {
                return $"{ReturnType} {Name}Async({string.Join(", ", Parameters.Select(x => x.ToString()))})";
            }
        }
    }

    public class ParameterDefinition
    {
        public string TypeName { get; set; }
        public string ParameterName { get; set; }
        public bool HasDefaultValue { get; set; }
        public string DefaultValue { get; set; }
        public IParameterSymbol OriginalSymbol { get; set; }

        public override string ToString()
        {
            if (HasDefaultValue)
            {
                return TypeName + " " + ParameterName + " = " + DefaultValue;
            }
            else
            {
                return TypeName + " " + ParameterName;
            }
        }
    }

    // MessagePack Definitions

    public interface IResolverRegisterInfo
    {
        string FullName { get; }
        string FormatterName { get; }
    }


    public class GenericSerializationInfo : IResolverRegisterInfo, IEquatable<GenericSerializationInfo>
    {
        public string FullName { get; set; }

        public string FormatterName { get; set; }

        public bool Equals(GenericSerializationInfo other)
        {
            return FullName.Equals(other.FullName);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    public class EnumSerializationInfo : IResolverRegisterInfo, IEquatable<EnumSerializationInfo>
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string UnderlyingType { get; set; }

        public string FormatterName => Name + "Formatter()";

        public bool Equals(EnumSerializationInfo other)
        {
            return FullName.Equals(other.FullName);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public override string ToString()
        {
            return FullName;
        }
    }



}
