using System.Diagnostics;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

[DebuggerDisplay("StreamingHub: {ServiceType,nq}; Methods={Methods.Count,nq}")]
public class MagicOnionStreamingHubInfo : IMagicOnionServiceInfo
{
    public MagicOnionTypeInfo ServiceType { get; }
    public IReadOnlyList<MagicOnionHubMethodInfo> Methods { get; }
    public MagicOnionStreamingHubReceiverInfo Receiver { get; }

    public MagicOnionStreamingHubInfo(MagicOnionTypeInfo serviceType, IReadOnlyList<MagicOnionHubMethodInfo> methods, MagicOnionStreamingHubReceiverInfo receiver)
    {
        ServiceType = serviceType;
        Methods = methods;
        Receiver = receiver;
    }

    [DebuggerDisplay("HubMethod: {MethodName,nq}; HubId={HubId,nq}; MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; Parameters={Parameters.Count,nq}")]
    public class MagicOnionHubMethodInfo
    {
        public int HubId { get; }
        public string MethodName { get; }
        public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; }

        /// <summary>
        /// Gets a type of method return value in the interface.
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

        public MagicOnionHubMethodInfo(int hubId, string methodName, IReadOnlyList<MagicOnionMethodParameterInfo> parameters, MagicOnionTypeInfo methodReturnType, MagicOnionTypeInfo requestType, MagicOnionTypeInfo responseType)
        {
            HubId = hubId;
            MethodName = methodName;
            Parameters = parameters;
            MethodReturnType = methodReturnType;
            RequestType = requestType;
            ResponseType = responseType;
        }
    }

    [DebuggerDisplay("HubMethod: {MethodName,nq}; HubId={HubId,nq}; MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; Parameters={Parameters.Count,nq}; IsClientResult={IsClientResult,nq}")]
    public class MagicOnionHubReceiverMethodInfo : MagicOnionHubMethodInfo
    {
        public bool IsClientResult { get; }

        public MagicOnionHubReceiverMethodInfo(int hubId, string methodName, IReadOnlyList<MagicOnionMethodParameterInfo> parameters, MagicOnionTypeInfo methodReturnType, MagicOnionTypeInfo requestType, MagicOnionTypeInfo responseType)
            : base(hubId, methodName, parameters, methodReturnType, requestType, responseType)
        {
            IsClientResult = methodReturnType != MagicOnionTypeInfo.KnownTypes.System_Void;
        }
    }

    [DebuggerDisplay("StreamingHubReceiver: {ReceiverType,nq}; Methods={Methods.Count,nq}")]
    public class MagicOnionStreamingHubReceiverInfo
    {
        public MagicOnionTypeInfo ReceiverType { get; }
        public IReadOnlyList<MagicOnionHubReceiverMethodInfo> Methods { get; }

        public MagicOnionStreamingHubReceiverInfo(MagicOnionTypeInfo receiverType, IReadOnlyList<MagicOnionHubReceiverMethodInfo> methods)
        {
            ReceiverType = receiverType;
            Methods = methods;
        }
    }
}
