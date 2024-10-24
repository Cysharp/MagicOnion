using System.Diagnostics.CodeAnalysis;
using MagicOnion.Internal.Reflection;

namespace MagicOnion.Client.DynamicClient
{
    [RequiresUnreferencedCode(nameof(DynamicClientAssemblyHolder) + " is incompatible with trimming and Native AOT.")]
#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        static class DynamicClientAssemblyHolder
    {
        public const string ModuleName = "MagicOnion.Client.DynamicClient";

        readonly static DynamicAssembly assembly;
        public static DynamicAssembly Assembly { get { return assembly; } }

        static DynamicClientAssemblyHolder()
        {
            assembly = new DynamicAssembly(ModuleName);
        }

#if ENABLE_SAVE_ASSEMBLY

        public static AssemblyBuilder Save()
        {
            return assembly.Save();
        }

#endif
    }
}
