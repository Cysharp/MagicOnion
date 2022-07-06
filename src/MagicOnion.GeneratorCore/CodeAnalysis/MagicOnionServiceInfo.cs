using System.Collections.Generic;
using System.Diagnostics;

namespace MagicOnion.Generator.CodeAnalysis
{
    [DebuggerDisplay("MagicOnionService: {ServiceType,nq}; Methods={Methods.Count,nq}")]
    public class MagicOnionServiceInfo : IMagicOnionServiceInfo
    {
        public MagicOnionTypeInfo ServiceType { get; }
        public IReadOnlyList<MagicOnionServiceMethodInfo> Methods { get; }

        public string IfDirectiveCondition { get; }
        public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

        public MagicOnionServiceInfo(MagicOnionTypeInfo serviceType, IReadOnlyList<MagicOnionServiceMethodInfo> methods, string ifDirectiveCondition)
        {
            ServiceType = serviceType;
            Methods = methods;
            IfDirectiveCondition = ifDirectiveCondition;
        }

        [DebuggerDisplay("ServiceMethod: {MethodName,nq} ({MethodType,nq} {Path,nq}); MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; Parameters={Parameters.Count,nq}")]
        public class MagicOnionServiceMethodInfo : IMagicOnionCompileDirectiveTarget
        {
            public MethodType MethodType { get; }
            public string ServiceName { get; }
            public string MethodName { get; }
            public string Path { get; }
            public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; } // T1, T2 ...
            public MagicOnionTypeInfo MethodReturnType { get; } // UnaryResult<T> or Task<{Server,Client,Duplex}Streaming<>>
            public MagicOnionTypeInfo RequestType { get; } // TArg or DynamicArgumentTuple<T1, T2 ...>
            public MagicOnionTypeInfo ResponseType { get; } // TResponse

            public string IfDirectiveCondition { get; }
            public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

            public MagicOnionServiceMethodInfo(
                MethodType methodType,
                string serviceName,
                string methodName,
                string path,
                IReadOnlyList<MagicOnionMethodParameterInfo> parameters,
                MagicOnionTypeInfo methodReturnType,
                MagicOnionTypeInfo requestType,
                MagicOnionTypeInfo responseType,
                string ifDirectiveCondition)
            {
                MethodType = methodType;
                ServiceName = serviceName;
                MethodName = methodName;
                Path = path;
                Parameters = parameters;
                MethodReturnType = methodReturnType;
                RequestType = requestType;
                ResponseType = responseType;
                IfDirectiveCondition = ifDirectiveCondition;
            }
        }
    }
}