using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Server.Hubs;
using MagicOnion.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace MagicOnion.Client
{
#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        static class StreamingHubClientAssemblyHolder
    {
        public const string ModuleName = "MagicOnion.Client.StreamingHubClient";

        readonly static DynamicAssembly assembly;
        public static ModuleBuilder Module { get { return assembly.ModuleBuilder; } }

        static StreamingHubClientAssemblyHolder()
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
        static class StreamingHubClientBuilder<TStreamingHub, TReceiver>
    {
        public static readonly Type ClientType;

        static readonly Type bytesMethod = typeof(Method<,>).MakeGenericType(new[] { typeof(byte[]), typeof(byte[]) });
        static readonly FieldInfo throughMarshaller = typeof(MagicOnionMarshallers).GetField("ThroughMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo nilBytes = typeof(MagicOnionMarshallers).GetField("UnsafeNilBytes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        static readonly MethodInfo callMessagePackDesrialize = typeof(LZ4MessagePackSerializer).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .First(x => x.Name == "Deserialize" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(ArraySegment<byte>));

        static StreamingHubClientBuilder()
        {
            var t = typeof(TStreamingHub);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + ti.Name);
            var module = StreamingHubClientAssemblyHolder.Module;
            var methodDefinitions = SearchDefinitions(t);

            var parentType = typeof(StreamingHubClientBase<,>).MakeGenericType(typeof(TStreamingHub), typeof(TReceiver));
            var typeBuilder = module.DefineType($"{DynamicClientAssemblyHolder.ModuleName}.{ti.FullName}StreamingHubClient_{Guid.NewGuid().ToString()}", TypeAttributes.Public, parentType, new Type[] { t });

            VerifyMethodDefinitions(typeBuilder, methodDefinitions);

            var methodField = DefineStaticConstructor(typeBuilder, t);
            DefineConstructor(typeBuilder, t, typeof(TReceiver));
            DefineMethods(typeBuilder, t, typeof(TReceiver), methodField, methodDefinitions);

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
                     || methodName == "DisposeAsync"
                     || methodName == "WaitForDisconnect"
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

        static void VerifyMethodDefinitions(TypeBuilder typeBuilder, MethodDefinition[] definitions)
        {
            var map = new Dictionary<int, MethodDefinition>(definitions.Length);
            foreach (var item in definitions)
            {
                var methodId = item.MethodInfo.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(item.MethodInfo.Name);
                if (map.ContainsKey(methodId))
                {
                    throw new Exception($"TReceiver does not allows duplicate methodId(hash code). Please change name or use MethodIdAttribute. {map[methodId].MethodInfo.Name} and {item.MethodInfo.Name}");
                }
                map.Add(methodId, item);

                if (!(item.MethodInfo.ReturnType.IsGenericType && item.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                   && item.MethodInfo.ReturnType != typeof(Task))
                {
                    throw new Exception($"Invalid definition, TReceiver's return type must only be `Task` or `Task<T>`. {item.MethodInfo.Name}.");
                }

                item.MethodId = methodId;
                if (item.RequestType == null)
                {
                    item.RequestType = MagicOnionMarshallers.CreateRequestType(item.MethodInfo.GetParameters());
                }
            }
        }

        static FieldInfo DefineStaticConstructor(TypeBuilder typeBuilder, Type interfaceType)
        {
            //  static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IFoo", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

            var field = typeBuilder.DefineField("method", bytesMethod, FieldAttributes.Private | FieldAttributes.Static);

            var cctor = typeBuilder.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
            var il = cctor.GetILGenerator();

            il.EmitLdc_I4((int)MethodType.DuplexStreaming);
            il.Emit(OpCodes.Ldstr, interfaceType.Name);
            il.Emit(OpCodes.Ldstr, "Connect");
            il.Emit(OpCodes.Ldsfld, throughMarshaller);
            il.Emit(OpCodes.Ldsfld, throughMarshaller);
            il.Emit(OpCodes.Newobj, bytesMethod.GetConstructors()[0]);
            il.Emit(OpCodes.Stsfld, field);

            il.Emit(OpCodes.Ret);

            return field;
        }

        static void DefineConstructor(TypeBuilder typeBuilder, Type interfaceType, Type receiverType)
        {
            // .ctor(CallInvoker callInvoker, string host, CallOptions option, IFormatterResolver resolver, ILogger logger) :base(...)
            {
                var argTypes = new[] { typeof(CallInvoker), typeof(string), typeof(CallOptions), typeof(IFormatterResolver), typeof(ILogger) };
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Ldarg_S, (byte)4);
                il.Emit(OpCodes.Ldarg_S, (byte)5);
                il.Emit(OpCodes.Call, typeBuilder.BaseType
                    .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First());
                il.Emit(OpCodes.Ret);
            }
        }

        static void DefineMethods(TypeBuilder typeBuilder, Type interfaceType, Type receiverType, FieldInfo methodField, MethodDefinition[] definitions)
        {
            var baseType = typeof(StreamingHubClientBase<,>).MakeGenericType(interfaceType, receiverType);
            var resolverField = baseType.GetField("resolver", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var receiverField = baseType.GetField("receiver", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // protected abstract Method<byte[], byte[]> DuplexStreamingAsyncMethod { get; }
            {
                var method = typeBuilder.DefineMethod("get_DuplexStreamingAsyncMethod", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    bytesMethod,
                    Type.EmptyTypes);
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, methodField);
                il.Emit(OpCodes.Ret);
            }
            {
                // Task DisposeAsync();
                {
                    var method = typeBuilder.DefineMethod("DisposeAsync", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, baseType.GetMethod("DisposeAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
                // Task WaitForDisconnect();
                {
                    var method = typeBuilder.DefineMethod("WaitForDisconnect", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WaitForDisconnect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                }
            }

            // receiver types borrow from DynamicBroadcastBuilder
            {
                // protected abstract void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data);
                {
                    var method = typeBuilder.DefineMethod("OnResponseEvent", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        null, new[] { typeof(int), typeof(object), typeof(ArraySegment<byte>) });
                    var il = method.GetILGenerator();

                    var labels = definitions
                        .Where(x => x.MethodInfo.ReturnType.IsGenericType) // only Task<T>
                        .Select(x => new { def = x, label = il.DefineLabel() })
                        .ToArray();

                    foreach (var item in labels)
                    {
                        // if( == ) goto ...
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitLdc_I4(item.def.MethodId);
                        il.Emit(OpCodes.Beq, item.label);
                    }
                    // else
                    il.Emit(OpCodes.Ret);

                    foreach (var item in labels)
                    {
                        // var result = LZ4MessagePackSerializer.Deserialize<T>(data, resolver);
                        // ((TaskCompletionSource<T>)taskCompletionSource).TrySetResult(result);

                        // => ((TaskCompletionSource<T>)taskCompletionSource).TrySetResult(LZ4MessagePackSerializer.Deserialize<T>(data, resolver));
                        var responseType = item.def.MethodInfo.ReturnType.GetGenericArguments()[0];
                        var tcsType = typeof(TaskCompletionSource<>).MakeGenericType(responseType);

                        il.MarkLabel(item.label);

                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Castclass, tcsType);

                        il.Emit(OpCodes.Ldarg_3);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, resolverField);
                        il.Emit(OpCodes.Call, callMessagePackDesrialize.MakeGenericMethod(responseType));

                        il.Emit(OpCodes.Callvirt, tcsType.GetMethod("TrySetResult"));
                        il.Emit(OpCodes.Pop);

                        il.Emit(OpCodes.Ret);
                    }
                }
                // protected abstract Task OnBroadcastEvent(int methodId, ArraySegment<byte> data);
                {
                    var methodDefinitions = BroadcasterHelper.SearchDefinitions(receiverType);
                    BroadcasterHelper.VerifyMethodDefinitions(methodDefinitions);

                    var method = typeBuilder.DefineMethod("OnBroadcastEvent", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), new[] { typeof(int), typeof(ArraySegment<byte>) });
                    var il = method.GetILGenerator();

                    var labels = methodDefinitions
                        .Select(x => new { def = x, label = il.DefineLabel() })
                        .ToArray();

                    foreach (var item in labels)
                    {
                        // if( == ) goto ...
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitLdc_I4(item.def.MethodId);
                        il.Emit(OpCodes.Beq, item.label);
                    }
                    // else
                    il.Emit(OpCodes.Call, typeof(Task).GetProperty("CompletedTask").GetGetMethod());
                    il.Emit(OpCodes.Ret);

                    foreach (var item in labels)
                    {
                        il.MarkLabel(item.label);

                        // var result = LZ4MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string>>(data, resolver);
                        // return receiver.OnReceiveMessage(result.Item1, result.Item2);

                        var parameters = item.def.MethodInfo.GetParameters();
                        if (parameters.Length == 0)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                            il.Emit(OpCodes.Ret);
                        }
                        else if (parameters.Length == 1)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, resolverField);
                            il.Emit(OpCodes.Call, callMessagePackDesrialize.MakeGenericMethod(parameters[0].ParameterType));
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                            il.Emit(OpCodes.Ret);
                        }
                        else
                        {
                            var deserializeType = BroadcasterHelper.dynamicArgumentTupleTypes[parameters.Length - 2]
                                .MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                            var lc = il.DeclareLocal(deserializeType);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, resolverField);
                            il.Emit(OpCodes.Call, callMessagePackDesrialize.MakeGenericMethod(deserializeType));
                            il.Emit(OpCodes.Stloc, lc);

                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                il.Emit(OpCodes.Ldloc, lc);
                                il.Emit(OpCodes.Ldfld, deserializeType.GetField("Item" + (i + 1)));
                            }

                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                            il.Emit(OpCodes.Ret);
                        }
                    }
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

                // return WriteMessageAsync<T>(methodId, message);
                // return WriteMessageWithResponseAsync<TReq, TRes>(methodId, message);

                // this.***
                il.Emit(OpCodes.Ldarg_0);

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
                    callType = def.RequestType;
                    il.Emit(OpCodes.Newobj, callType.GetConstructors().First());
                }

                if (def.MethodInfo.ReturnType == typeof(Task))
                {
                    var mInfo = baseType.GetMethod("WriteMessageAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType));
                }
                else
                {
                    var mInfo = baseType.GetMethod("WriteMessageWithResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType, def.MethodInfo.ReturnType.GetGenericArguments()[0]));
                }

                il.Emit(OpCodes.Ret);
            }
        }

        class MethodDefinition
        {
            public string Path => ServiceType.Name + "/" + MethodInfo.Name;

            public Type ServiceType;
            public MethodInfo MethodInfo;
            public int MethodId;

            public Type RequestType;
        }
    }
}
