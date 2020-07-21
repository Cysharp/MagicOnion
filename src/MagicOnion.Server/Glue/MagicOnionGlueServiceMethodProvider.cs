using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;

namespace MagicOnion.Server.Glue
{
    internal class MagicOnionGlueServiceMethodProvider<TService> : MagicOnionGlueServiceMethodProviderBase, IServiceMethodProvider<TService>
        where TService : class
    {
        private static readonly MethodInfo bindServiceMethod = typeof(ServerServiceDefinition).GetMethod("BindService", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public MagicOnionGlueServiceMethodProvider(MagicOnionServiceDefinition magicOnionServerServiceDefinition) : base(magicOnionServerServiceDefinition.ServerServiceDefinition)
        {
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var binder = new MagicOnionGlueServiceBinder<TService>(context);
            bindServiceMethod.Invoke(ServerServiceDefinition, new[] { binder });
        }
    }
}
