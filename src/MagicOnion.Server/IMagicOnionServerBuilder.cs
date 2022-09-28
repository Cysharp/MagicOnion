using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
