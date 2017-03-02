using Grpc.Core;
using MagicOnion.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
    internal static class AssemblyHolder
    {
        public const string ModuleName = "MagicOnion.Client.DynamicClient";

        readonly static DynamicAssembly assembly;
        public static ModuleBuilder Module { get { return assembly.ModuleBuilder; } }

        static AssemblyHolder()
        {
            assembly = new DynamicAssembly(ModuleName);
        }
    }

    internal static class DynamicClientBuilder<T>
    {
        public static readonly Type ClientType;
        static readonly Type bytesMethod = typeof(Method<,>).MakeGenericType(new[] { typeof(byte[]), typeof(byte[]) });
        static readonly FieldInfo byteArrayMarshaller = typeof(MagicOnionMarshallers).GetField("ByteArrayMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo emptyBytes = typeof(MagicOnionMarshallers).GetField("EmptyBytes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        static DynamicClientBuilder()
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + ti.Name);

            var resolverType = typeof(TTypeResolver);
            var module = AssemblyHolder.Module;
            var methodDefinitions = SearchDefinitions(t);

            var parentType = typeof(MagicOnionClientBase<>).MakeGenericType(t);
            var typeBuilder = module.DefineType($"{AssemblyHolder.ModuleName}.{resolverType.Name}.{ti.FullName}Client", TypeAttributes.Public, parentType, new Type[] { t });

            DefineStaticFields(typeBuilder, methodDefinitions);
            DefineStaticConstructor(typeBuilder, resolverType, t, methodDefinitions);
            var emptyCtor = DefineConstructors(typeBuilder, methodDefinitions);
            DefineMethods(typeBuilder, resolverType, t, methodDefinitions, emptyCtor);

            ClientType = typeBuilder.CreateTypeInfo().AsType();
        }

        static MethodDefinition[] SearchDefinitions(Type interfaceType)
        {
            return interfaceType
                .GetInterfaces()
                .Concat(new []{ interfaceType })
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
                item.FieldResponseMarshaller = typeBuilder.DefineField(item.MethodInfo.Name + "ResponseMarshaller", typeof(Marshaller<>).MakeGenericType(item.ResponseType), FieldAttributes.Private | FieldAttributes.Static);

                if (item.RequestType == null)
                {
                    item.RequestType = MagicOnionMarshallers.CreateRequestType(item.MethodInfo.GetParameters());
                }
                item.FieldRequestMarshaller = typeBuilder.DefineField(item.MethodInfo.Name + "RequestMarshaller", typeof(Marshaller<>).MakeGenericType(item.RequestType), FieldAttributes.Private | FieldAttributes.Static);
            }
        }

        static void DefineStaticConstructor(TypeBuilder typeBuilder, Type resolverType, Type interfaceType, MethodDefinition[] definitions)
        {
            var cctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
            var il = cctor.GetILGenerator();

            for (int i = 0; i < definitions.Length; i++)
            {
                il.DeclareLocal(typeof(object)); // object _marshaller
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];

                il.EmitLdc_I4((int)def.MethodType);
                il.Emit(OpCodes.Ldstr, def.ServiceType.Name);
                il.Emit(OpCodes.Ldstr, def.MethodInfo.Name);
                il.Emit(OpCodes.Ldsfld, byteArrayMarshaller);
                il.Emit(OpCodes.Ldsfld, byteArrayMarshaller);
                il.Emit(OpCodes.Newobj, bytesMethod.GetConstructors()[0]);
                il.Emit(OpCodes.Stsfld, def.FieldMethod);

                if (def.MethodType == MethodType.Unary || def.MethodType == MethodType.ServerStreaming)
                {
                    il.Emit(OpCodes.Ldtoken, resolverType);
                    il.Emit(OpCodes.Call, getTypeFromHandle);
                    il.Emit(OpCodes.Ldstr, def.Path);
                    il.Emit(OpCodes.Ldtoken, def.MethodInfo.DeclaringType);
                    il.Emit(OpCodes.Call, getTypeFromHandle);
                    il.Emit(OpCodes.Ldstr, def.MethodInfo.Name);
                    il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetMethod", new[] { typeof(string) }));
                    il.Emit(OpCodes.Callvirt, typeof(MethodBase).GetMethod("GetParameters"));
                    il.EmitLdloca(i);
                    il.Emit(OpCodes.Call, typeof(MagicOnionMarshallers).GetMethod("CreateRequestTypeAndMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
                    il.Emit(OpCodes.Pop);
                    il.EmitLdloc(i);
                }
                else
                {
                    il.Emit(OpCodes.Call, typeof(Formatter<,>).MakeGenericType(resolverType, def.RequestType).GetProperty("Default").GetGetMethod());
                    il.Emit(OpCodes.Call, typeof(MagicOnionMarshallers).GetMethod("CreateZeroFormatterMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(resolverType, def.RequestType));
                }
                il.Emit(OpCodes.Castclass, def.FieldRequestMarshaller.FieldType);
                il.Emit(OpCodes.Stsfld, def.FieldRequestMarshaller);

                il.Emit(OpCodes.Call, typeof(Formatter<,>).MakeGenericType(resolverType, def.ResponseType).GetProperty("Default").GetGetMethod());
                il.Emit(OpCodes.Call, typeof(MagicOnionMarshallers).GetMethod("CreateZeroFormatterMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(resolverType, def.ResponseType));
                il.Emit(OpCodes.Stsfld, def.FieldResponseMarshaller);
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

            ConstructorInfo invokerCtor;
            // .ctor(CallInvoker):base(callInvoker)
            {
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(CallInvoker) });
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeBuilder.BaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(CallInvoker) }, null));
                il.Emit(OpCodes.Ret);

                invokerCtor = ctor;
            }
            // .ctor(Channel):this(new DefaultCallInvoker(channel))
            {
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(Channel) });
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Newobj, typeof(DefaultCallInvoker).GetConstructor(new[] { typeof(Channel) }));
                il.Emit(OpCodes.Call, invokerCtor);
                il.Emit(OpCodes.Ret);
            }

            return emptyCtor;
        }

        static void DefineMethods(TypeBuilder typeBuilder, Type resolverType, Type interfaceType, MethodDefinition[] definitions, ConstructorInfo emptyCtor)
        {
            var baseType = typeof(MagicOnionClientBase<>).MakeGenericType(interfaceType);
            var hostField = baseType.GetField("host", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var optionField = baseType.GetField("option", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var invokerField = baseType.GetField("callInvoker", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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

                        il.Emit(OpCodes.Ldsfld, def.FieldRequestMarshaller);
                        il.Emit(OpCodes.Callvirt, def.FieldRequestMarshaller.FieldType.GetProperty("Serializer").GetGetMethod());
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            il.Emit(OpCodes.Ldarg, j + 1);
                        }
                        if (parameters.Length == 0)
                        {
                            // use empty byte[0]
                            il.Emit(OpCodes.Ldsfld, emptyBytes);
                        }
                        else if (parameters.Length == 1)
                        {
                            // already loaded parameter.
                        }
                        else
                        {
                            il.Emit(OpCodes.Newobj, def.RequestType.GetConstructors()[0]);
                        }
                        il.Emit(OpCodes.Callvirt, def.FieldRequestMarshaller.FieldType.GetProperty("Serializer").PropertyType.GetMethod("Invoke"));
                        il.Emit(OpCodes.Stloc_0);

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

                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldsfld, def.FieldResponseMarshaller);
                        if (def.MethodType == MethodType.Unary)
                        {
                            il.Emit(OpCodes.Newobj, typeof(UnaryResult<>).MakeGenericType(def.ResponseType).GetConstructors()[0]);
                        }
                        else
                        {
                            il.Emit(OpCodes.Newobj, typeof(ServerStreamingResult<>).MakeGenericType(def.ResponseType).GetConstructors()[0]);
                        }
                        if (def.ResponseIsTask)
                        {
                            il.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(UnaryResult<>).MakeGenericType(def.ResponseType)));
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

                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldsfld, def.FieldRequestMarshaller);
                        il.Emit(OpCodes.Ldsfld, def.FieldResponseMarshaller);

                        if (def.MethodType == MethodType.ClientStreaming)
                        {
                            il.Emit(OpCodes.Newobj, typeof(ClientStreamingResult<,>).MakeGenericType(def.RequestType, def.ResponseType).GetConstructors()[0]);
                        }
                        else
                        {
                            il.Emit(OpCodes.Newobj, typeof(DuplexStreamingResult<,>).MakeGenericType(def.RequestType, def.ResponseType).GetConstructors()[0]);
                        }
                        if (def.ResponseIsTask)
                        {
                            il.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(ClientStreamingResult<,>).MakeGenericType(def.RequestType, def.ResponseType)));
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
            if (!t.IsGenericType) throw new Exception($"Invalid return type, path:{def.Path} type:{t.Name}");

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
                //methodType = MethodType.Unary; // TODO:others...
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
            public FieldInfo FieldRequestMarshaller;
            public Type ResponseType;
            public FieldInfo FieldResponseMarshaller;
        }
    }
}