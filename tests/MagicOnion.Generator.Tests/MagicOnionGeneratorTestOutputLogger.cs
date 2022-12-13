using MagicOnion.Generator.Internal;
using Xunit.Abstractions;

namespace MagicOnion.Generator.Tests;

public class MagicOnionGeneratorTestOutputLogger : IMagicOnionGeneratorLogger
{
    readonly ITestOutputHelper outputHelper;

    public MagicOnionGeneratorTestOutputLogger(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }

#if TRUE
    public void Trace(string message) => outputHelper.WriteLine(message);
#else
    public void Trace(string message) {}
#endif
    public void Information(string message) => outputHelper.WriteLine(message);
    public void Error(string message, Exception exception = null) => outputHelper.WriteLine(message);
}
