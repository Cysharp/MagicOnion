using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator.CodeGen.Extensions;

public static class MagicOnionMethodParameterInfoExtensions
{
    // "TArg1 arg1, TArg2 arg2 ..."
    public static string ToMethodSignaturize(this IEnumerable<MagicOnionMethodParameterInfo> parameters)
        => string.Join(", ", parameters.Select((x, i) => $"{x.Type.FullName} {x.Name}"));

    public static string ToNewDynamicArgumentTuple(this IEnumerable<MagicOnionMethodParameterInfo> parameters)
        => $"""new global::MagicOnion.DynamicArgumentTuple<{string.Join(", ", parameters.Select((x, i) => $"{x.Type.FullName}"))}>({string.Join(", ", parameters.Select((x, i) => x.Name))})""";
}
