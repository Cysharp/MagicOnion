using System;

namespace MagicOnion.Generator.Internal;

public interface IMagicOnionGeneratorLogger
{
    void Trace(string message);
    void Information(string message);
    void Error(string message, Exception exception = null);
}

public class MagicOnionGeneratorNullLogger : IMagicOnionGeneratorLogger
{
    public static IMagicOnionGeneratorLogger Instance { get; } = new MagicOnionGeneratorNullLogger();

    public void Trace(string message)
    {
    }

    public void Information(string message)
    {
    }

    public void Error(string message, Exception exception = null)
    {
    }
}

public class MagicOnionGeneratorConsoleLogger : IMagicOnionGeneratorLogger
{
    readonly bool verbose;

    public MagicOnionGeneratorConsoleLogger(bool verbose)
    {
        this.verbose = verbose;
    }
        
    public void Trace(string message)
    {
        if (verbose)
        {
            Console.WriteLine(message);
        }
    }

    public void Information(string message)
    {
        Console.WriteLine(message);
    }

    public void Error(string message, Exception exception = null)
    {
        Console.Error.WriteLine(message);
    }
}
