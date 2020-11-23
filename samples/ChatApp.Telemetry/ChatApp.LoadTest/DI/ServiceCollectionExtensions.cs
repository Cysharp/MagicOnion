using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatApp.LoadTest.DI
{
    static class ServiceCollectionExtensions
    {
        public static void AddFactory<TService, TImplementation>(this IServiceCollection services)
             where TService : class
             where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddSingleton<Func<TService>>(x => () => x.GetService<TService>());
        }

        public static void AddFactory<TService>(this IServiceCollection services)
        where TService : class
        {
            services.AddTransient<TService>();
            services.AddSingleton<Func<TService>>(x => () => x.GetService<TService>());
        }
    }
}
