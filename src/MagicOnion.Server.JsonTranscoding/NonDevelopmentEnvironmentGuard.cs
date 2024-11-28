using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.JsonTranscoding;

internal class NonDevelopmentEnvironmentGuard(IHostEnvironment hostEnvironment, IOptions<MagicOnionJsonTranscodingOptions> transcodingOptions) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!transcodingOptions.Value.AllowEnableInNonDevelopmentEnvironment && !hostEnvironment.IsDevelopment())
        {
            throw new InvalidOperationException($"MagicOnion.Server.JsonTranscoding should not be enabled in non-development environment. If you want to enable it, you need to set `{nameof(MagicOnionJsonTranscodingOptions.AllowEnableInNonDevelopmentEnvironment)}` to `true`.");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
