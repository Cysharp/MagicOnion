using System;
using System.Linq;
using MagicOnion.Server;
using MagicOnion.Server.Authentication.Jwt;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MagicOnionAuthenticationJwtServiceCollectionExtensions
    {
        /// <summary>
        /// Adds JWT authentication and provider for MagicOnion to the specified <see cref="IMagicOnionServerBuilder"/>.
        /// </summary>
        /// <typeparam name="TProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IMagicOnionServerBuilder AddJwtAuthentication<TProvider>(this IMagicOnionServerBuilder builder, Action<JwtAuthenticationOptions> configureOptions)
            where TProvider: class, IJwtAuthenticationProvider
        {
            builder.Services.AddSingleton<IJwtAuthenticationProvider, TProvider>();

            builder.Services.Configure<JwtAuthenticationOptions>(configureOptions);
            builder.Services.Configure<MagicOnionOptions>(options =>
            {
                options.GlobalFilters.Add<JwtAuthenticationAttribute>();
            });

            return builder;
        }
    }
}