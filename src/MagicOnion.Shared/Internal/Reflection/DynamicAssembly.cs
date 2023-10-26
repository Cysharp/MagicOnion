#if NON_UNITY || ((!ENABLE_IL2CPP || UNITY_EDITOR) && !NET_STANDARD_2_0)

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MagicOnion.Internal.Reflection
{
#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        class DynamicAssembly
    {
        readonly object gate = new object();

#if ENABLE_SAVE_ASSEMBLY
        readonly string moduleName;
#endif
        readonly AssemblyBuilder assemblyBuilder;
        readonly ModuleBuilder moduleBuilder;

        // don't expose ModuleBuilder(should use lock)
        // public ModuleBuilder ModuleBuilder { get { return moduleBuilder; } }

        public DynamicAssembly(string moduleName)
        {
#if ENABLE_SAVE_ASSEMBLY
            this.moduleName = moduleName;
            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.RunAndSave);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName, moduleName + ".dll");
#else
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
#endif

            // HACK: Allow access to `internal` classes from dynamically generated assembly.
            // https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/
            assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(IgnoresAccessChecksToAttribute).GetConstructor(new[] { typeof(string) })!, new[] { "MagicOnion.Client" }));
            assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(IgnoresAccessChecksToAttribute).GetConstructor(new[] { typeof(string) })!, new[] { "MagicOnion.Server" }));
        }

        // requires lock on mono environment(for example, UnityEditor). see: https://github.com/neuecc/MessagePack-CSharp/issues/161

        public TypeBuilder DefineType(string name, TypeAttributes attr)
        {
            lock (gate)
            {
                return moduleBuilder.DefineType(name, attr);
            }
        }

        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {
            lock (gate)
            {
                return moduleBuilder.DefineType(name, attr, parent);
            }
        }

        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            lock (gate)
            {
                return moduleBuilder.DefineType(name, attr, parent, interfaces);
            }
        }

#if ENABLE_SAVE_ASSEMBLY

        public AssemblyBuilder Save()
        {
            assemblyBuilder.Save(moduleName + ".dll");
            return assemblyBuilder;
        }

#endif
    }
}


namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}

#endif
