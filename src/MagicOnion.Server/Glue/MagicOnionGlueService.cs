using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Grpc.Core;

namespace MagicOnion.Server.Glue
{
    internal class MagicOnionGlueService
    {
        public static Type CreateType()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicModule");
            var typeBuilder = dynamicModule.DefineType("MagicOnionGlue");
            var type = typeBuilder.CreateType()!;
            return typeof(MagicOnionGlueService<>).MakeGenericType(type);
        }

        public static void BindMethod(ServiceBinderBase binder, MagicOnionGlueService service)
        {
            // no-op at here.
            // The MagicOnion service methods are bound by `MagicOnionGlueServiceMethodProvider<TService>`
        }
    }

    [BindServiceMethod(typeof(MagicOnionGlueService), nameof(BindMethod))]
    internal class MagicOnionGlueService<TService> : MagicOnionGlueService
        where TService : class
    {
    }
}
