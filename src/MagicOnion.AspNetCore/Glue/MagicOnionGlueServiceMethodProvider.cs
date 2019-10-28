using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;

namespace MagicOnion.AspNetCore
{
    internal class MagicOnionGlueServiceMethodProvider<TService> : MagicOnionGlueServiceMethodProviderBase, IServiceMethodProvider<TService>
        where TService : class
    {
        private static readonly MethodInfo bindServiceMethod = typeof(ServerServiceDefinition).GetMethod("BindService", BindingFlags.Instance | BindingFlags.NonPublic);

        public MagicOnionGlueServiceMethodProvider(ServerServiceDefinition serverServiceDefinition) : base(serverServiceDefinition)
        {
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var binder = new MagicOnionGlueServiceBinder<TService>(context);
            bindServiceMethod.Invoke(ServerServiceDefinition, new [] { binder });
        }
    }
}