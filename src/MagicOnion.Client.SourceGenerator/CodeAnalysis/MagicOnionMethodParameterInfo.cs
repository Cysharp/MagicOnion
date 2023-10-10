using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

[DebuggerDisplay("Parameter: {Name,nq} ({Type,nq})")]
public class MagicOnionMethodParameterInfo
{
    public string Name { get; }
    public MagicOnionTypeInfo Type { get; }
    public bool HasExplicitDefaultValue { get; }
    public string DefaultValue { get; }

    public MagicOnionMethodParameterInfo(string name, MagicOnionTypeInfo type, bool hasExplicitDefaultValue, string defaultValue)
    {
        Name = name;
        Type = type;
        HasExplicitDefaultValue = hasExplicitDefaultValue;
        DefaultValue = defaultValue;
    }

    public static MagicOnionMethodParameterInfo CreateFromSymbol(IParameterSymbol parameterSymbol)
    {
        var type = MagicOnionTypeInfo.CreateFromSymbol(parameterSymbol.Type);
        return new MagicOnionMethodParameterInfo(parameterSymbol.Name, type, parameterSymbol.HasExplicitDefaultValue, GetDefaultValue(parameterSymbol));
    }

    static string GetDefaultValue(IParameterSymbol p)
    {
        if (p.HasExplicitDefaultValue)
        {
            var ppp = p.ToDisplayParts(new SymbolDisplayFormat(parameterOptions: SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue));

            if (!ppp.Any(x => x.Kind == SymbolDisplayPartKind.Keyword && x.ToString() == "default"))
            {
                var l = ppp.Last();
                if (l.Kind == SymbolDisplayPartKind.FieldName)
                {
                    return l.Symbol!.ToDisplayString();
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
