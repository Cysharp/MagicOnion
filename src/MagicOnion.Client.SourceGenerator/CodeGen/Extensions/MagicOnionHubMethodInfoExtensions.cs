using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen.Extensions;

public static class MagicOnionHubMethodInfoExtensions
{
    public static string ToMethodSignature(this MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo methodInfo)
        => $"public {methodInfo.MethodReturnType.FullName} {methodInfo.MethodName}({string.Join(", ", methodInfo.Parameters.Select(x => $"{x.Type.FullName} {x.Name}"))})";

    public static string ToHubFireAndForgetWriteMessage(this MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo methodInfo)
    {
        var requestObject = methodInfo.Parameters.Count == 0
            ? "global::MessagePack.Nil.Default"
            : methodInfo.Parameters.Count == 1
                ? methodInfo.Parameters[0].Name
                : $"new global::MagicOnion.DynamicArgumentTuple<{string.Join(", ", methodInfo.Parameters.Select(x => x.Type.FullName))}>({string.Join(", ", methodInfo.Parameters.Select(x => x.Name))})";

        return $"WriteMessageWithResponseAsync<{methodInfo.RequestType.FullName}, {methodInfo.ResponseType.FullName}>({methodInfo.HubId}, {requestObject})";
    }

    public static string ToHubWriteMessage(this MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo methodInfo)
    {
        var requestObject = methodInfo.Parameters.Count == 0
            ? "global::MessagePack.Nil.Default"
            : methodInfo.Parameters.Count == 1
                ? methodInfo.Parameters[0].Name
                : $"new global::MagicOnion.DynamicArgumentTuple<{string.Join(", ", methodInfo.Parameters.Select(x => x.Type.FullName))}>({string.Join(", ", methodInfo.Parameters.Select(x => x.Name))})";

        return $"WriteMessageWithResponseAsync<{methodInfo.RequestType.FullName}, {methodInfo.ResponseType.FullName}>({methodInfo.HubId}, {requestObject})";
    }

    public static (string line1, string line2) ToHubOnBroadcastMessage(this MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo methodInfo)
    {
        string parameterType;
        string line2;
        if (methodInfo.Parameters.Count == 0)
        {
            parameterType = "global::MessagePack.Nil";
            line2 = $"{methodInfo.MethodName}()";
        }
        else if (methodInfo.Parameters.Count == 1)
        {
            parameterType = methodInfo.Parameters[0].Type.FullName;
            line2 = $"{methodInfo.MethodName}(result)";
        }
        else
        {
            var typeArgs = string.Join(", ", methodInfo.Parameters.Select(x => x.Type.FullName));
            parameterType = $"global::MagicOnion.DynamicArgumentTuple<{typeArgs}>";
            line2 = string.Join(", ", Enumerable.Range(1, methodInfo.Parameters.Count).Select(x => $"result.Item{x}"));
            line2 = $"{methodInfo.MethodName}({line2})";
        }

        line2 = "receiver." + line2 + "; break;";

        var line1 = $"var result = MessagePackSerializer.Deserialize<{parameterType}>(data, serializerOptions);";
        return (line1, line2);
    }

    public static (string line1, string line2) ToHubOnResponseEvent(this MagicOnionStreamingHubInfo.MagicOnionHubMethodInfo methodInfo)
    {
        var line1 = $"var result = MessagePackSerializer.Deserialize<{methodInfo.ResponseType.FullName}>(data, serializerOptions);";
        var line2 = $"((TaskCompletionSource<{methodInfo.ResponseType.FullName}>)taskCompletionSource).TrySetResult(result);";
        return (line1, line2);
    }
}
