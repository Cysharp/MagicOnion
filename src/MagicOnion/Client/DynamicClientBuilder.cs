#if NON_UNITY || !NET_STANDARD_2_0

using Grpc.Core;
using MagicOnion.Utils;
using MessagePack;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    internal static class DynamicClientAssemblyHolder
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

    internal static class DynamicClientBuilder<T>
    {
        public static readonly Type ClientType;
        static readonly Type bytesMethod = typeof(Method<,>).MakeGenericType(new[] { typeof(byte[]), typeof(byte[]) });
        static readonly FieldInfo throughMarshaller = typeof(MagicOnionMarshallers).GetField("ThroughMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo nilBytes = typeof(MagicOnionMarshallers).GetField("UnsafeNilBytes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo callMessagePackSerialize = typeof(LZ4MessagePackSerializer).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .First(x => x.Name == "Serialize" && x.GetParameters().Length == 2 && x.ReturnType == typeof(byte[]));

        static DynamicClientBuilder()
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + ti.Name);

            var asm = DynamicClientAssemblyHolder.Assembly;
            var methodDefinitions = SearchDefinitions(t);

            var parentType = typeof(MagicOnionClientBase<>).MakeGenericType(t);
            var typeBuilder = asm.DefineType($"{DynamicClientAssemblyHolder.ModuleName}.{ti.FullName}Client", TypeAttributes.Public, parentType, new Type[] { t });

            DefineStaticFields(typeBuilder, methodDefinitions);
            DefineStaticConstructor(typeBuilder, t, methodDefinitions);
            var emptyCtor = DefineConstructors(typeBuilder, methodDefinitions);
            DefineMethods(typeBuilder, t, methodDefinitions, emptyCtor);

            ClientType = typeBuilder.CreateTypeInfo().AsType();
        }

        static MethodDefinition[] SearchDefinitions(Type interfaceType)
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
                     || methodName == "WithOptions"
                     || methodName == "WithHeaders"
                     || methodName == "WithDeadline"
                     || methodName == "WithCancellationToken"
                     || methodName == "WithHost"
                     )
                    {
                        return false;
                    }
                    return true;
                })
                .Where(x => !x.IsSpecialName)
                .Select(x => new MethodDefinition
                {
                    ServiceType = interfaceType,
                    MethodInfo = x,
                })
                .ToArray();
        }

        static void DefineStaticFields(TypeBuilder typeBuilder, MethodDefinition[] definitions)
        {
            foreach (var item in definitions)
            {
                item.FieldMethod = typeBuilder.DefineField(item.MethodInfo.Name + "Method", bytesMethod, FieldAttributes.Private | FieldAttributes.Static);
                item.ResponseType = UnwrapResponseType(item, out item.MethodType, out item.ResponseIsTask, out item.RequestType);

                if (item.RequestType == null)
                {
                    item.RequestType = MagicOnionMarshallers.CreateRequestType(item.MethodInfo.GetParameters());
                }
            }
        }

        static void DefineStaticConstructor(TypeBuilder typeBuilder, Type interfaceType, MethodDefinition[] definitions)
        {
            var cctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
            var il = cctor.GetILGenerator();

            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];

                il.EmitLdc_I4((int)def.MethodType);
                il.Emit(OpCodes.Ldstr, def.ServiceType.Name);
                il.Emit(OpCodes.Ldstr, def.MethodInfo.Name);
                il.Emit(OpCodes.Ldsfld, throughMarshaller);
                il.Emit(OpCodes.Ldsfld, throughMarshaller);
                il.Emit(OpCodes.Newobj, bytesMethod.GetConstructors()[0]);
                il.Emit(OpCodes.Stsfld, def.FieldMethod);
            }

            il.Emit(OpCodes.Ret);
        }

        static ConstructorInfo DefineConstructors(TypeBuilder typeBuilder, MethodDefinition[] definitions)
        {
            ConstructorInfo emptyCtor;
            // .ctor()
            {
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Private, CallingConventions.Standard, Type.EmptyTypes);
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));
                il.Emit(OpCodes.Ret);

                emptyCtor = ctor;
            }

            // .ctor(CallInvoker, IFormatterResolver):base(callInvoker, resolver)
            {
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(CallInvoker), typeof(IFormatterResolver) });
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(CallInvoker), typeof(IFormatterResolver) }, null));
                il.Emit(OpCodes.Ret);
            }

            return emptyCtor;
        }

        static void DefineMethods(TypeBuilder typeBuilder, Type interfaceType, MethodDefinition[] definitions, ConstructorInfo emptyCtor)
        {
            var baseType = typeof(MagicOnionClientBase<>).MakeGenericType(interfaceType);
            var hostField = baseType.GetField("host", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var optionField = baseType.GetField("option", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var invokerField = baseType.GetField("callInvoker", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var resolverField = baseType.GetField("resolver", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Clone
            {
                var method = typeBuilder.DefineMethod("Clone", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    typeof(MagicOnionClientBase<>).MakeGenericType(interfaceType),
                    Type.EmptyTypes);
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Newobj, emptyCtor);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, hostField);
                il.Emit(OpCodes.Stfld, hostField);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, optionField);
                il.Emit(OpCodes.Stfld, optionField);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, invokerField);
                il.Emit(OpCodes.Stfld, invokerField);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, resolverField);
                il.Emit(OpCodes.Stfld, resolverField);

                il.Emit(OpCodes.Ret);
            }
            // Overrides
            {
                // TSelf WithOption(CallOptions option)
                {
                    var method = typeBuilder.DefineMethod("WithOptions", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType,
                        new[] { typeof(CallOptions) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WithOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
                // TSelf WithHeaders(Metadata headers);
                {
                    var method = typeBuilder.DefineMethod("WithHeaders", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType,
                        new[] { typeof(Metadata) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WithHeaders", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
                // TSelf WithDeadline(DateTime deadline);
                {
                    var method = typeBuilder.DefineMethod("WithDeadline", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType,
                        new[] { typeof(DateTime) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WithDeadline", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
                // TSelf WithCancellationToken(CancellationToken cancellationToken);
                {
                    var method = typeBuilder.DefineMethod("WithCancellationToken", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType,
                        new[] { typeof(CancellationToken) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WithCancellationToken", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
                // TSelf WithHost(string host);
                {
                    var method = typeBuilder.DefineMethod("WithHost", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType,
                        new[] { typeof(string) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WithHost", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
            }
            // Proxy Methods
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                var parameters = def.MethodInfo.GetParameters().Select(x => x.ParameterType).ToArray();

                var method = typeBuilder.DefineMethod(def.MethodInfo.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    def.MethodInfo.ReturnType,
                    parameters);
                var il = method.GetILGenerator();

                switch (def.MethodType)
                {
                    case MethodType.Unary:
                    case MethodType.ServerStreaming:
                        il.DeclareLocal(typeof(byte[])); // request
                        if (def.MethodType == MethodType.Unary)
                        {
                            il.DeclareLocal(typeof(AsyncUnaryCall<byte[]>)); // callResult
                        }
                        else
                        {
                            il.DeclareLocal(typeof(AsyncServerStreamingCall<byte[]>));
                        }

                        // create request
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            il.Emit(OpCodes.Ldarg, j + 1);
                        }
                        if (parameters.Length == 0)
                        {
                            // use empty byte[0]
                            il.Emit(OpCodes.Ldsfld, nilBytes);
                        }
                        else if (parameters.Length == 1)
                        {
                            // already loaded parameter.
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, resolverField);
                            il.Emit(OpCodes.Call, callMessagePackSerialize.MakeGenericMethod(def.RequestType));
                        }
                        else
                        {
                            // call new DynamicArgumentTuple<T>
                            il.Emit(OpCodes.Newobj, def.RequestType.GetConstructors()[0]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, resolverField);
                            il.Emit(OpCodes.Call, callMessagePackSerialize.MakeGenericMethod(def.RequestType));
                        }
                        il.Emit(OpCodes.Stloc_0);

                        // create ***Result
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, invokerField);
                        il.Emit(OpCodes.Ldsfld, def.FieldMethod);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, hostField);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, optionField);
                        il.Emit(OpCodes.Ldloc_0);
                        if (def.MethodType == MethodType.Unary)
                        {
                            il.Emit(OpCodes.Callvirt, typeof(CallInvoker).GetMethod("AsyncUnaryCall").MakeGenericMethod(typeof(byte[]), typeof(byte[])));
                        }
                        else
                        {
                            il.Emit(OpCodes.Callvirt, typeof(CallInvoker).GetMethod("AsyncServerStreamingCall").MakeGenericMethod(typeof(byte[]), typeof(byte[])));
                        }
                        il.Emit(OpCodes.Stloc_1);

                        // create return result
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, resolverField);
                        Type resultType;
                        if (def.MethodType == MethodType.Unary)
                        {
                            resultType = typeof(UnaryResult<>).MakeGenericType(def.ResponseType);
                            il.Emit(OpCodes.Newobj, resultType.GetConstructors().OrderBy(x => x.GetParameters().Length).Last());
                        }
                        else
                        {
                            resultType = typeof(ServerStreamingResult<>).MakeGenericType(def.ResponseType);
                            il.Emit(OpCodes.Newobj, resultType.GetConstructors()[0]);
                        }
                        if (def.ResponseIsTask)
                        {
                            il.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult").MakeGenericMethod(resultType));
                        }
                        break;
                    case MethodType.ClientStreaming:
                    case MethodType.DuplexStreaming:
                        if (def.MethodType == MethodType.ClientStreaming)
                        {
                            il.DeclareLocal(typeof(AsyncClientStreamingCall<byte[], byte[]>)); // callResult
                        }
                        else
                        {
                            il.DeclareLocal(typeof(AsyncDuplexStreamingCall<byte[], byte[]>)); // callResult
                        }

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, invokerField);
                        il.Emit(OpCodes.Ldsfld, def.FieldMethod);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, hostField);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, optionField);
                        if (def.MethodType == MethodType.ClientStreaming)
                        {
                            il.Emit(OpCodes.Callvirt, typeof(CallInvoker).GetMethod("AsyncClientStreamingCall").MakeGenericMethod(typeof(byte[]), typeof(byte[])));
                        }
                        else
                        {
                            il.Emit(OpCodes.Callvirt, typeof(CallInvoker).GetMethod("AsyncDuplexStreamingCall").MakeGenericMethod(typeof(byte[]), typeof(byte[])));
                        }
                        il.Emit(OpCodes.Stloc_0);

                        // create return result
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, resolverField);
                        Type resultType2;
                        if (def.MethodType == MethodType.ClientStreaming)
                        {
                            resultType2 = typeof(ClientStreamingResult<,>).MakeGenericType(def.RequestType, def.ResponseType);
                            il.Emit(OpCodes.Newobj, resultType2.GetConstructors().OrderBy(x => x.GetParameters().Length).Last());
                        }
                        else
                        {
                            resultType2 = typeof(DuplexStreamingResult<,>).MakeGenericType(def.RequestType, def.ResponseType);
                            il.Emit(OpCodes.Newobj, resultType2.GetConstructors()[0]);
                        }

                        if (def.ResponseIsTask)
                        {
                            il.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult").MakeGenericMethod(resultType2));
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Not supported method type:" + def.MethodType);
                }

                il.Emit(OpCodes.Ret);
            }
        }

        static Type UnwrapResponseType(MethodDefinition def, out MethodType methodType, out bool responseIsTask, out Type requestTypeIfExists)
        {
            //if (!t.IsGenericType) throw new Exception($"Invalid ResponseType, Path:{def.Path} Type:{t.Name}");

            var t = def.MethodInfo.ReturnType;
            if (!t.GetTypeInfo().IsGenericType) throw new Exception($"Invalid return type, path:{def.Path} type:{t.Name}");

            // Task<Unary<T>>
            if (t.GetGenericTypeDefinition() == typeof(Task<>))
            {
                responseIsTask = true;
                t = t.GetGenericArguments()[0];
            }
            else
            {
                responseIsTask = false;
            }

            // Unary<T>
            var returnType = t.GetGenericTypeDefinition();
            if (returnType == typeof(UnaryResult<>))
            {
                methodType = MethodType.Unary;
                requestTypeIfExists = null;
                return t.GetGenericArguments()[0];
            }
            else if (returnType == typeof(ClientStreamingResult<,>))
            {
                methodType = MethodType.ClientStreaming;
                var genArgs = t.GetGenericArguments();
                requestTypeIfExists = genArgs[0];
                return genArgs[1];
            }
            else if (returnType == typeof(ServerStreamingResult<>))
            {
                methodType = MethodType.ServerStreaming;
                requestTypeIfExists = null;
                return t.GetGenericArguments()[0];
            }
            else if (returnType == typeof(DuplexStreamingResult<,>))
            {
                methodType = MethodType.DuplexStreaming;
                var genArgs = t.GetGenericArguments();
                requestTypeIfExists = genArgs[0];
                return genArgs[1];
            }
            else
            {
                throw new Exception($"Invalid return type, path:{def.Path} type:{t.Name}");
            }
        }

        class MethodDefinition
        {
            public string Path => ServiceType.Name + "/" + MethodInfo.Name;

            // set after search definitions
            public Type ServiceType;
            public MethodInfo MethodInfo;
            public MethodType MethodType;

            // set after define static fields
            public bool ResponseIsTask;
            public FieldInfo FieldMethod;
            public Type RequestType;
            public Type ResponseType;
        }
    }
}

#endif