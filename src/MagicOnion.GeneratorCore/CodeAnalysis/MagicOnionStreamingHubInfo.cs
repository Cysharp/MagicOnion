using System.Collections.Generic;
using System.Diagnostics;

namespace MagicOnion.GeneratorCore.CodeAnalysis
{
    [DebuggerDisplay("StreamingHub: {ServiceType,nq}; Methods={Methods.Count,nq}")]
    public class MagicOnionStreamingHubInfo : IMagicOnionCompileDirectiveTarget
    {
        public MagicOnionTypeInfo ServiceType { get; }
        public IReadOnlyList<MagicOnionHubMethodInfo> Methods { get; }

        public string IfDirectiveCondition { get; }
        public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);

        [DebuggerDisplay("HubMethod: {MethodName,nq}; HubId={HubId,nq}; MethodReturnType={MethodReturnType,nq}; RequestType={RequestType,nq}; ResponseType={ResponseType,nq}; ParameterTypes={ParameterTypes.Count,nq}")]
        public class MagicOnionHubMethodInfo : IMagicOnionCompileDirectiveTarget
        {
            public int HubId { get; }
            public string MethodName { get; }
            public IReadOnlyList<MagicOnionTypeInfo> ParameterTypes { get; }
            public MagicOnionTypeInfo MethodReturnType { get; }
            public MagicOnionTypeInfo RequestType { get; }
            public MagicOnionTypeInfo ResponseType { get; }

            public string IfDirectiveCondition { get; }
            public bool HasIfDirectiveCondition => !string.IsNullOrEmpty(IfDirectiveCondition);
        }
    }
}