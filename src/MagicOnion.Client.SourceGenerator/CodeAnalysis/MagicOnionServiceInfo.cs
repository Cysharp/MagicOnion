using System.Diagnostics;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

[DebuggerDisplay("MagicOnionService: {ServiceType,nq}; Methods={Methods.Count,nq}")]
public class MagicOnionServiceInfo : IMagicOnionServiceInfo
{
    public MagicOnionTypeInfo ServiceType { get; }
    public string ServiceName { get; }
    public IReadOnlyList<MagicOnionServiceMethodInfo> Methods { get; }

    public MagicOnionServiceInfo(MagicOnionTypeInfo serviceType, string serviceName, IReadOnlyList<MagicOnionServiceMethodInfo> methods)
    {
        ServiceType = serviceType;
        ServiceName = serviceName;
        Methods = methods;
    }

    [DebuggerDisplay("ServiceMethod: {MethodName,nq} ({MethodType,nq} {Path,nq}); MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; Parameters={Parameters.Count,nq}")]
    public class MagicOnionServiceMethodInfo
    {
        public MethodType MethodType { get; }
        public string ServiceName { get; }
        public string MethodName { get; }
        public string Path { get; }
        public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; } // T1, T2 ...

        /// <summary>
        /// Gets a type of method return value in the interface. ex. <c>UnaryResult&lt;T></c>, <c>Task&lt;ServerStreamingResult></c>, <c>Task&lt;ClientStreamingResult></c> or <c>Task&lt;DuplexStreamingResult></c>.
        /// </summary>
        public MagicOnionTypeInfo MethodReturnType { get; }

        /// <summary>
        /// Gets a type for serializing method parameters.
        /// <list type="bullet">
        /// <item>
        ///     <term>The method has no parameter</term>
        ///     <description><c>MessagePack.Nil</c></description>
        /// </item>
        /// <item>
        ///     <term>The method has exact one parameter</term>
        ///     <description><c>Parameters[0].Type</c></description>
        /// </item>
        /// <item>
        ///     <term>The method has two or more parameters</term>
        ///     <description><c>DynamicArgumentTuple&lt;Parameters[0].Type, Parameters[1].Type ...></c></description>
        /// </item>
        /// </list>
        /// </summary>
        public MagicOnionTypeInfo RequestType { get; }

        /// <summary>
        /// Gets a type for serializing method return value.
        /// <list type="bullet">
        /// <item>
        ///     <term>The method has no return value. (void, Task and ValueTask)</term>
        ///     <description><c>MessagePack.Nil</c></description>
        /// </item>
        /// <item>
        ///     <term>The method has return value. (Task&lt;T>)</term>
        ///     <description><c>MethodReturnType.GenericArguments[0]</c></description>
        /// </item>
        /// <item>
        ///     <term>The method has return value.</term>
        ///     <description><c>MethodReturnType</c></description>
        /// </item>
        /// </list>
        /// </summary>
        public MagicOnionTypeInfo ResponseType { get; }

        public MagicOnionServiceMethodInfo(
            MethodType methodType,
            string serviceName,
            string methodName,
            string path,
            IReadOnlyList<MagicOnionMethodParameterInfo> parameters,
            MagicOnionTypeInfo methodReturnType,
            MagicOnionTypeInfo requestType,
            MagicOnionTypeInfo responseType)
        {
            MethodType = methodType;
            ServiceName = serviceName;
            MethodName = methodName;
            Path = path;
            Parameters = parameters;
            MethodReturnType = methodReturnType;
            RequestType = requestType;
            ResponseType = responseType;
        }
    }
}
