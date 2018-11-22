using MagicOnion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace MagicOnion.Server.Hubs
{
#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        static class AssemblyHolder
    {
        public const string ModuleName = "MagicOnion.Server.Hubs.DynamicBroadcaster";

        readonly static DynamicAssembly assembly;
        public static ModuleBuilder Module { get { return assembly.ModuleBuilder; } }

        static AssemblyHolder()
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

#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        static class DynamicBroadcasterBuilder<T>
    {
        public static readonly Type BroadcasterType;
        public static readonly Type BroadcasterType_ExceptOne;
        public static readonly Type BroadcasterType_ExceptMany;

        // start from -2
        static readonly Type[] dynamicArgumentTupleTypes = typeof(DynamicArgumentTuple<,>).GetTypeInfo().Assembly
            .GetTypes()
            .Where(x => x.Name.StartsWith("DynamicArgumentTuple") && !x.Name.Contains("Formatter"))
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();

        static readonly MethodInfo groupWriteAllMethodInfo = typeof(IGroup).GetMethod(nameof(IGroup.WriteAllAsync));
        static readonly MethodInfo groupWriteExceptOneMethodInfo = typeof(IGroup).GetMethods().First(x => x.Name == nameof(IGroup.WriteExceptAsync) && !x.GetParameters().Last().ParameterType.IsArray);
        static readonly MethodInfo groupWriteExceptManyMethodInfo = typeof(IGroup).GetMethods().First(x => x.Name == nameof(IGroup.WriteExceptAsync) && x.GetParameters().Last().ParameterType.IsArray);

        static DynamicBroadcasterBuilder()
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Broadcaster Proxy only allows interface. Type:" + ti.Name);

            var module = AssemblyHolder.Module;
            var methodDefinitions = BroadcasterHelper.SearchDefinitions(t);
            BroadcasterHelper.VerifyMethodDefinitions(methodDefinitions);

            {
                var typeBuilder = module.DefineType($"{AssemblyHolder.ModuleName}.{ti.FullName}Broadcaster_{Guid.NewGuid().ToString()}", TypeAttributes.Public, typeof(object), new Type[] { t });
                var (group, ctor) = DefineConstructor(typeBuilder);
                DefineMethods(typeBuilder, t, group, methodDefinitions, groupWriteAllMethodInfo, null);
                BroadcasterType = typeBuilder.CreateTypeInfo().AsType();
            }
            {
                var typeBuilder = module.DefineType($"{AssemblyHolder.ModuleName}.{ti.FullName}BroadcasterExceptOne_{Guid.NewGuid().ToString()}", TypeAttributes.Public, typeof(object), new Type[] { t });
                var (group, except, ctor) = DefineConstructor2(typeBuilder);
                DefineMethods(typeBuilder, t, group, methodDefinitions, groupWriteExceptOneMethodInfo, except);
                BroadcasterType_ExceptOne = typeBuilder.CreateTypeInfo().AsType();
            }
            {
                var typeBuilder = module.DefineType($"{AssemblyHolder.ModuleName}.{ti.FullName}BroadcasterExceptMany_{Guid.NewGuid().ToString()}", TypeAttributes.Public, typeof(object), new Type[] { t });
                var (group, except, ctor) = DefineConstructor3(typeBuilder);
                DefineMethods(typeBuilder, t, group, methodDefinitions, groupWriteExceptManyMethodInfo, except);
                BroadcasterType_ExceptMany = typeBuilder.CreateTypeInfo().AsType();
            }
        }

        static (FieldInfo, ConstructorInfo) DefineConstructor(TypeBuilder typeBuilder)
        {
            // .ctor(IGroup group)
            var groupField = typeBuilder.DefineField("group", typeof(IGroup), FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IGroup) });
            var il = ctor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetConstructors().First());
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, groupField);
            il.Emit(OpCodes.Ret);

            return (groupField, ctor);
        }

        static (FieldInfo groupField, FieldInfo exceptField, ConstructorInfo) DefineConstructor2(TypeBuilder typeBuilder)
        {
            // .ctor(IGroup group, Guid except)
            var groupField = typeBuilder.DefineField("group", typeof(IGroup), FieldAttributes.Private | FieldAttributes.InitOnly);
            var exceptField = typeBuilder.DefineField("except", typeof(Guid), FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IGroup), typeof(Guid) });
            var il = ctor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetConstructors().First());
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, groupField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, exceptField);
            il.Emit(OpCodes.Ret);

            return (groupField, exceptField, ctor);
        }

        static (FieldInfo groupField, FieldInfo exceptField, ConstructorInfo) DefineConstructor3(TypeBuilder typeBuilder)
        {
            // .ctor(IGroup group, Guid[] except)
            var groupField = typeBuilder.DefineField("group", typeof(IGroup), FieldAttributes.Private | FieldAttributes.InitOnly);
            var exceptField = typeBuilder.DefineField("excepts", typeof(Guid[]), FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IGroup), typeof(Guid[]) });
            var il = ctor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetConstructors().First());
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, groupField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, exceptField);
            il.Emit(OpCodes.Ret);

            return (groupField, exceptField, ctor);
        }

        static void DefineMethods(TypeBuilder typeBuilder, Type interfaceType, FieldInfo groupField, BroadcasterHelper.MethodDefinition[] definitions, MethodInfo writeMethod, FieldInfo exceptField)
        {
            // Proxy Methods
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                var parameters = def.MethodInfo.GetParameters().Select(x => x.ParameterType).ToArray();

                var method = typeBuilder.DefineMethod(def.MethodInfo.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    def.MethodInfo.ReturnType,
                    parameters);
                var il = method.GetILGenerator();

                // like this.
                // return group.WriteAllAsync(9013131, new DynamicArgumentTuple<int, string>(senderId, message));

                // load group field
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, groupField);

                // arg1
                il.EmitLdc_I4(def.MethodId);

                // create request for arg2
                for (int j = 0; j < parameters.Length; j++)
                {
                    il.Emit(OpCodes.Ldarg, j + 1);
                }

                Type callType = null;
                if (parameters.Length == 0)
                {
                    // use null.
                    callType = typeof(byte[]);
                    il.Emit(OpCodes.Ldnull);
                }
                else if (parameters.Length == 1)
                {
                    // already loaded parameter.
                    callType = parameters[0];
                }
                else
                {
                    // call new DynamicArgumentTuple<T>
                    callType = dynamicArgumentTupleTypes[parameters.Length - 2].MakeGenericType(parameters);
                    il.Emit(OpCodes.Newobj, callType.GetConstructors().First());
                }

                if (writeMethod != groupWriteAllMethodInfo)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, exceptField);
                }

                il.Emit(OpCodes.Callvirt, writeMethod.MakeGenericMethod(callType));
                il.Emit(OpCodes.Ret);
            }
        }
    }

    internal static class BroadcasterHelper
    {
        internal static MethodDefinition[] SearchDefinitions(Type interfaceType)
        {
            return interfaceType
                .GetInterfaces()
                .Concat(new[] { interfaceType })
                .SelectMany(x => x.GetMethods())
                .Where(x =>
                {
                    var methodInfo = x;
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) return false;
                    if (methodInfo.GetCustomAttribute<IgnoreAttribute>(false) != null) return false; // ignore

                    var methodName = methodInfo.Name;
                    if (methodName == "Equals"
                     || methodName == "GetHashCode"
                     || methodName == "GetType"
                     || methodName == "ToString"
                     )
                    {
                        return false;
                    }
                    return true;
                })
                .Where(x => !x.IsSpecialName)
                .Select(x => new MethodDefinition
                {
                    ReceiverType = interfaceType,
                    MethodInfo = x,
                })
                .ToArray();
        }

        internal static void VerifyMethodDefinitions(MethodDefinition[] definitions)
        {
            // define and set.
            var map = new Dictionary<int, MethodDefinition>(definitions.Length);
            foreach (var item in definitions)
            {
                var methodId = item.MethodInfo.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(item.MethodInfo.Name);
                if (map.ContainsKey(methodId))
                {
                    throw new Exception($"TReceiver does not allows duplicate methodId(hash code). Please change name or use MethodIdAttribute. {map[methodId].MethodInfo.Name} and {item.MethodInfo.Name}");
                }
                map.Add(methodId, item);

                if (item.MethodInfo.ReturnType != typeof(Task))
                {
                    throw new Exception($"Invalid definition, TReceiver's return type must only be `Task`. {item.MethodInfo.Name}.");
                }

                item.MethodId = methodId;
            }
        }

        public class MethodDefinition
        {
            public string Path => ReceiverType.Name + "/" + MethodInfo.Name;

            public Type ReceiverType;
            public MethodInfo MethodInfo;
            public int MethodId;
        }
    }
}