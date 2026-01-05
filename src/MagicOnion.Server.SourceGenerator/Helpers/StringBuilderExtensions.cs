using System.Runtime.CompilerServices;
using System.Text;

namespace MagicOnion.Server.SourceGenerator;

internal static class StringBuilderExtensions
{
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
