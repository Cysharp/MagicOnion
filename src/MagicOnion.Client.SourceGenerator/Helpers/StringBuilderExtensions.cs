using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace MagicOnion.Client.SourceGenerator;

static class StringBuilderExtensions
{
    // NOTE: .NET Standard 2.0 doesn't have AppendInterpolatedStringHandler
    public static StringBuilder AppendLineWithFormat(this StringBuilder sb, [InterpolatedStringHandlerArgument(nameof(sb))] StringBuilderInterpolatedStringHandler s)
    {
        sb.AppendLine();
        return sb;
    }

    [InterpolatedStringHandler]
    public readonly ref struct StringBuilderInterpolatedStringHandler
    {
        readonly StringBuilder sb;

        public StringBuilderInterpolatedStringHandler(int literalLength, int formattedCount, StringBuilder sb)
        {
            this.sb = sb;
        }

        public void AppendLiteral(string s)
        {
            sb.Append(s);
        }

        public void AppendFormatted<T>(T t)
        {
            sb.Append(t);
        }
    }
}
