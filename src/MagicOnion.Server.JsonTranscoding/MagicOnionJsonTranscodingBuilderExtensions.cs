using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server;
using MagicOnion.Server.JsonTranscoding;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionJsonTranscodingBuilderExtensions
{
    /// <summary>
    /// Adds JSON transcoding support to the MagicOnion server.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IMagicOnionServerBuilder AddJsonTranscoding(this IMagicOnionServerBuilder builder, Action<MagicOnionJsonTranscodingOptions>? configureOptions = default)
    {
        builder.Services.AddOptions<MagicOnionJsonTranscodingOptions>();
        builder.Services.AddSingleton(typeof(IServiceMethodProvider<>), typeof(MagicOnionJsonTranscodingGrpcServiceMethodProvider<>));

        if (configureOptions is not null)
        {
            builder.Services.PostConfigure(configureOptions);
        }

        builder.Services.AddHostedService<NonDevelopmentEnvironmentGuard>();

        return builder;
    }
}
