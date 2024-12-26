using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server.JsonTranscoding.Tests;

public class JsonTranscodingEnabledMagicOnionApplicationFactory<T> : MagicOnionApplicationFactory<T>
{
    protected override void OnConfigureMagicOnionBuilder(IMagicOnionServerBuilder builder)
    {
        builder.AddJsonTranscoding();
    }
}

public abstract class JsonTranscodingEnabledMagicOnionApplicationFactory : MagicOnionApplicationFactory
{
    protected override void OnConfigureMagicOnionBuilder(IMagicOnionServerBuilder builder)
    {
        builder.AddJsonTranscoding();
    }
}
