using MagicOnion;
using MagicOnion.Serialization.MessagePack;
using MessagePack;
using MessagePack.Formatters;

namespace AotSample.Server;

/// <summary>
/// Custom MessagePack resolver for AOT support.
/// Registers DynamicArgumentTupleFormatter for methods with multiple parameters.
/// </summary>
public class MagicOnionAotFormatterResolver : IFormatterResolver
{
    public static readonly MagicOnionAotFormatterResolver Instance = new();

    private MagicOnionAotFormatterResolver() { }

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            Formatter = (IMessagePackFormatter<T>?)MagicOnionAotFormatters.GetFormatter(typeof(T));
        }
    }
}

/// <summary>
/// Contains formatter instances for DynamicArgumentTuple types used in this project.
/// Add formatters here for any service methods with 2+ parameters.
/// </summary>
public static class MagicOnionAotFormatters
{
    // DynamicArgumentTuple formatters for methods with multiple parameters
    // AddAsync(int a, int b) -> DynamicArgumentTuple<int, int>
    private static readonly DynamicArgumentTupleFormatter<int, int> _intIntFormatter = 
        new(default, default);

    // OnMessage(string user, string message) -> DynamicArgumentTuple<string, string>
    private static readonly DynamicArgumentTupleFormatter<string, string> _stringStringFormatter =
        new(default!, default!);

    public static object? GetFormatter(Type type)
    {
        // DynamicArgumentTuple<int, int> for AddAsync(int a, int b)
        if (type == typeof(DynamicArgumentTuple<int, int>))
        {
            return _intIntFormatter;
        }

        // DynamicArgumentTuple<string, string> for OnMessage(string user, string message)
        if (type == typeof(DynamicArgumentTuple<string, string>))
        {
            return _stringStringFormatter;
        }

        return null;
    }
}
