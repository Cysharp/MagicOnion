using Grpc.AspNetCore.Server.Model;
using MagicOnion.Server;
using MagicOnion.Server.JsonTranscoding;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MagicOnionJsonTranscodingBuilderExtensions
{
    public static IMagicOnionServerBuilder AddJsonTranscoding(this IMagicOnionServerBuilder builder, Action<MagicOnionJsonTranscodingOptions>? configureOptions = default)
    {
        builder.Services.AddOptions<MagicOnionJsonTranscodingOptions>();
        builder.Services.AddSingleton(typeof(IServiceMethodProvider<>), typeof(MagicOnionJsonTranscodingGrpcServiceMethodProvider<>));

        if (configureOptions is not null)
        {
            builder.Services.PostConfigure(configureOptions);
        }

        return builder;
    }
}
