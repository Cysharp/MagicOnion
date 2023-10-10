using System.CodeDom.Compiler;

namespace MagicOnion.Client.SourceGenerator.Internal;

internal static class IndentedTextWriterExtensions
{
    public static void WriteLines(this IndentedTextWriter writer, string lines)
    {
        foreach (var line in lines.Split('\n'))
        {
            writer.WriteLine(line.TrimEnd());
        }
    }

    public static IndentedBlock BeginIndent(this IndentedTextWriter textWriter, int depth = 1)
    {
        textWriter.Indent += depth;
        return new IndentedBlock(textWriter, depth);
    }

    public readonly struct IndentedBlock : IDisposable
    {
        readonly IndentedTextWriter textWriter;
        readonly int depth;

        public IndentedBlock(IndentedTextWriter textWriter, int depth)
        {
            this.textWriter = textWriter;
            this.depth = depth;
        }

        public void Dispose()
        {
            textWriter.Indent -= depth;
        }
    }

    public static IfBlock IfDirective(this IndentedTextWriter textWriter, string conditions)
    {
        if (!string.IsNullOrWhiteSpace(conditions))
        {
            textWriter.WriteLineNoTabs($"#if {conditions}");
        }
        return new IfBlock(textWriter, conditions);
    }

    public readonly struct IfBlock : IDisposable
    {
        readonly IndentedTextWriter textWriter;
        readonly string conditions;

        public IfBlock(IndentedTextWriter textWriter, string conditions)
        {
            this.textWriter = textWriter;
            this.conditions = conditions;
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(conditions))
            {
                textWriter.WriteLineNoTabs($"#endif // {conditions}");
            }
        }
    }
}
