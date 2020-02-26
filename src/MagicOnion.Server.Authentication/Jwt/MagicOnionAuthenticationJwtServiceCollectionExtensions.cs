using System;
using System.Linq;
using MagicOnion.Hosting;
using MagicOnion.Server;
using MagicOnion.Server.Authentication.Jwt;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MagicOnionAuthenticationJwtServiceCollectionExtensions
    {
        public static IServiceCollection AddMagicOnionJwtAuthentication<TProvider>(this IServiceCollection services, Action<JwtAuthenticationOptions> configureOptions)
            where TProvider: class, IJwtAuthenticationProvider
        {
            services.AddSingleton<IJwtAuthenticationProvider, TProvider>();

            services.Configure<JwtAuthenticationOptions>(configureOptions);
            services.Configure<MagicOnionHostingOptions>(options =>
            {
                options.Service.GlobalFilters.Add<JwtAuthenticationAttribute>();
            });

            return services;
        }
    }
}