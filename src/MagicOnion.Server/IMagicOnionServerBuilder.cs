using Microsoft.Extensions.DependencyInjection;

namespace MagicOnion.Server;

public interface IMagicOnionServerBuilder
{
    IServiceCollection Services { get; }
}

internal class MagicOnionServerBuilder : IMagicOnionServerBuilder
{
    public IServiceCollection Services { get; }

    public MagicOnionServerBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
