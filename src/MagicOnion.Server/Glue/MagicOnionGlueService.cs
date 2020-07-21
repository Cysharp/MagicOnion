using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MagicOnion.Server.Glue
{
    internal static class MagicOnionGlueService
    {
        public static Type CreateType()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicModule");
            var typeBuilder = dynamicModule.DefineType("MagicOnionGlue");
            var type = typeBuilder.CreateType()!;
            return typeof(MagicOnionGlueService<>).MakeGenericType(type);
        }
    }

    internal class MagicOnionGlueService<TService>
        where TService : class
    {
    }
}
