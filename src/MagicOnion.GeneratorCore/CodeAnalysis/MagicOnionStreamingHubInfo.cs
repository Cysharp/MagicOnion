using System.Collections.Generic;
using System.Diagnostics;

namespace MagicOnion.Generator.CodeAnalysis;

[DebuggerDisplay("StreamingHub: {ServiceType,nq}; Methods={Methods.Count,nq}")]
public class MagicOnionStreamingHubInfo : IMagicOnionServiceInfo
{
    public MagicOnionTypeInfo ServiceType { get; }
    public IReadOnlyList<MagicOnionHubMethodInfo> Methods { get; }
    public MagicOnionStreamingHubReceiverInfo Receiver { get; }

    public string IfDirectiveCondition { get; }
    public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

    public MagicOnionStreamingHubInfo(MagicOnionTypeInfo serviceType, IReadOnlyList<MagicOnionHubMethodInfo> methods, MagicOnionStreamingHubReceiverInfo receiver, string ifDirectiveCondition)
    {
        ServiceType = serviceType;
        Methods = methods;
        Receiver = receiver;
        IfDirectiveCondition = ifDirectiveCondition;
    }

    [DebuggerDisplay("HubMethod: {MethodName,nq}; HubId={HubId,nq}; MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; Parameters={Parameters.Count,nq}")]
    public class MagicOnionHubMethodInfo : IMagicOnionCompileDirectiveTarget
    {
        public int HubId { get; }
        public string MethodName { get; }
        public IReadOnlyList<MagicOnionMethodParameterInfo> Parameters { get; }
        public MagicOnionTypeInfo MethodReturnType { get; }
        public MagicOnionTypeInfo RequestType { get; }
        public MagicOnionTypeInfo ResponseType { get; }

        public string IfDirectiveCondition { get; }
        public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

        public MagicOnionHubMethodInfo(int hubId, string methodName, IReadOnlyList<MagicOnionMethodParameterInfo> parameters, MagicOnionTypeInfo methodReturnType, MagicOnionTypeInfo requestType, MagicOnionTypeInfo responseType, string ifDirectiveCondition)
        {
            HubId = hubId;
            MethodName = methodName;
            Parameters = parameters;
            MethodReturnType = methodReturnType;
            RequestType = requestType;
            ResponseType = responseType;
            IfDirectiveCondition = ifDirectiveCondition;
        }
    }

    [DebuggerDisplay("StreamingHubReceiver: {ReceiverType,nq}; Methods={Methods.Count,nq}")]
    public class MagicOnionStreamingHubReceiverInfo : IMagicOnionCompileDirectiveTarget
    {
        public MagicOnionTypeInfo ReceiverType { get; }
        public IReadOnlyList<MagicOnionHubMethodInfo> Methods { get; }

        public string IfDirectiveCondition { get; }
        public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

        public MagicOnionStreamingHubReceiverInfo(MagicOnionTypeInfo receiverType, IReadOnlyList<MagicOnionHubMethodInfo> methods, string ifDirectiveCondition)
        {
            ReceiverType = receiverType;
            Methods = methods;
            IfDirectiveCondition = ifDirectiveCondition;
        }
    }
}
