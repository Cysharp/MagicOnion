#if NON_UNITY || !NET_STANDARD_2_0

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
using System.Threading;
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
        internal static DynamicAssembly Assembly { get { return assembly; } }

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
        // static readonly Type ClientFireAndForgetType;

        static readonly Type bytesMethod = typeof(Method<,>).MakeGenericType(new[] { typeof(byte[]), typeof(byte[]) });
        static readonly FieldInfo throughMarshaller = typeof(MagicOnionMarshallers).GetField("ThroughMarshaller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        static readonly ConstructorInfo notSupportedException = typeof(NotSupportedException).GetConstructor(Type.EmptyTypes);

        static readonly MethodInfo callMessagePackDesrialize = typeof(MessagePackSerializer).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .First(x => x.Name == "Deserialize" && x.GetParameters().Length == 3 && x.GetParameters()[0].ParameterType == typeof(ReadOnlyMemory<byte>) && x.GetParameters()[1].ParameterType == typeof(MessagePackSerializerOptions));
        static readonly MethodInfo callCancellationTokenNone = typeof(CancellationToken).GetProperty("None", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod();
        static readonly PropertyInfo completedTask = typeof(Task).GetProperty("CompletedTask", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        static StreamingHubClientBuilder()
        {
            var t = typeof(TStreamingHub);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + ti.Name);
            var asm = StreamingHubClientAssemblyHolder.Assembly;
            var methodDefinitions = SearchDefinitions(t);

            var parentType = typeof(StreamingHubClientBase<,>).MakeGenericType(typeof(TStreamingHub), typeof(TReceiver));
            var typeBuilder = asm.DefineType($"{DynamicClientAssemblyHolder.ModuleName}.{ti.FullName}StreamingHubClient_{Guid.NewGuid().ToString()}", TypeAttributes.Public, parentType, new Type[] { t });

            VerifyMethodDefinitions(methodDefinitions);

            {
                // Create FireAndForgetType first as nested type.
                var typeBuilderEx = typeBuilder.DefineNestedType($"FireAndForgetClient", TypeAttributes.NestedPrivate, typeof(object), new Type[] { t });
                var tuple = DefineFireAndForgetConstructor(typeBuilderEx, typeBuilder);
                var fireAndForgetClientCtor = tuple.Item1;
                var fireAndForgetField = tuple.Item2;

                DefineMethodsFireAndForget(typeBuilderEx, t, fireAndForgetField, typeBuilder, methodDefinitions);
                typeBuilderEx.CreateTypeInfo(); // ok to create nested type.

                var methodField = DefineStaticConstructor(typeBuilder, t);
                var clientField = DefineConstructor(typeBuilder, t, typeof(TReceiver), fireAndForgetClientCtor);
                DefineMethods(typeBuilder, t, typeof(TReceiver), methodField, clientField, methodDefinitions);
            }

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
                     || methodName == "FireAndForget"
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

        static void VerifyMethodDefinitions(MethodDefinition[] definitions)
        {
            var map = new Dictionary<int, MethodDefinition>(definitions.Length);
            foreach (var item in definitions)
            {
                var methodId = item.MethodInfo.GetCustomAttribute<MethodIdAttribute>()?.MethodId ?? FNV1A32.GetHashCode(item.MethodInfo.Name);
                if (map.ContainsKey(methodId))
                {
                    throw new Exception($"TStreamingHub does not allows duplicate methodId(hash code). Please change name or use MethodIdAttribute. {map[methodId].MethodInfo.Name} and {item.MethodInfo.Name}");
                }
                map.Add(methodId, item);

                if (!(item.MethodInfo.ReturnType.IsGenericType && item.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                   && item.MethodInfo.ReturnType != typeof(Task))
                {
                    throw new Exception($"Invalid definition, TStreamingHub's return type must only be `Task` or `Task<T>`. {item.MethodInfo.Name}.");
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

        static FieldInfo DefineConstructor(TypeBuilder typeBuilder, Type interfaceType, Type receiverType, ConstructorInfo fireAndForgetClientCtor)
        {
            // .ctor(CallInvoker callInvoker, string host, CallOptions option, MessagePackSerializerOptions resolver, ILogger logger) :base(...)
            {
                var argTypes = new[] { typeof(CallInvoker), typeof(string), typeof(CallOptions), typeof(MessagePackSerializerOptions), typeof(ILogger) };
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

                // { this.fireAndForgetClient = new FireAndForgetClient(this); }
                var clientField = typeBuilder.DefineField("fireAndForgetClient", fireAndForgetClientCtor.DeclaringType, FieldAttributes.Private);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, fireAndForgetClientCtor);
                il.Emit(OpCodes.Stfld, clientField);

                il.Emit(OpCodes.Ret);

                return clientField;
            }
        }

        static Tuple<ConstructorBuilder, FieldBuilder> DefineFireAndForgetConstructor(TypeBuilder typeBuilder, Type parentClientType)
        {
            // .ctor(Parent client) { this.client = client }
            {
                var argTypes = new[] { parentClientType };
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);
                var clientField = typeBuilder.DefineField("client", parentClientType, FieldAttributes.Private);
                var il = ctor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructors().First());

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, clientField);

                il.Emit(OpCodes.Ret);

                return Tuple.Create(ctor, clientField);
            }
        }

        static void DefineMethods(TypeBuilder typeBuilder, Type interfaceType, Type receiverType, FieldInfo methodField, FieldInfo clientField, MethodDefinition[] definitions)
        {
            var baseType = typeof(StreamingHubClientBase<,>).MakeGenericType(interfaceType, receiverType);
            var serializerOptionsField = baseType.GetField("serializerOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
                // TSelf FireAndForget();
                {
                    var method = typeBuilder.DefineMethod("FireAndForget", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType, Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, clientField);
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

                        Type responseType;
                        Type tcsType;
                        if (item.def.MethodInfo.ReturnType == typeof(Task))
                        {
                            // Task methods uses TaskCompletionSource<Nil>
                            responseType = typeof(Nil);
                            tcsType = typeof(TaskCompletionSource<Nil>);
                        }
                        else
                        {
                            responseType = item.def.MethodInfo.ReturnType.GetGenericArguments()[0];
                            tcsType = typeof(TaskCompletionSource<>).MakeGenericType(responseType);
                        }

                        il.MarkLabel(item.label);

                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Castclass, tcsType);
                        il.Emit(OpCodes.Ldarg_3);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, serializerOptionsField);
                        il.Emit(OpCodes.Call, callCancellationTokenNone);
                        il.Emit(OpCodes.Call, callMessagePackDesrialize.MakeGenericMethod(responseType));

                        il.Emit(OpCodes.Callvirt, tcsType.GetMethod("TrySetResult"));
                        il.Emit(OpCodes.Pop);

                        il.Emit(OpCodes.Ret);
                    }
                }
                // protected abstract void OnBroadcastEvent(int methodId, ArraySegment<byte> data);
                {
                    var methodDefinitions = BroadcasterHelper.SearchDefinitions(receiverType);
                    BroadcasterHelper.VerifyMethodDefinitions(methodDefinitions);

                    var method = typeBuilder.DefineMethod("OnBroadcastEvent", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(void), new[] { typeof(int), typeof(ArraySegment<byte>) });
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
                            // TODO:fix emit
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, serializerOptionsField);
                            il.Emit(OpCodes.Call, callCancellationTokenNone);
                            il.Emit(OpCodes.Call, callMessagePackDesrialize.MakeGenericMethod(parameters[0].ParameterType));
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                            il.Emit(OpCodes.Ret);
                        }
                        else
                        {
                            // TODO:fix emit
                            var deserializeType = BroadcasterHelper.dynamicArgumentTupleTypes[parameters.Length - 2]
                                .MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                            var lc = il.DeclareLocal(deserializeType);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, serializerOptionsField);
                            il.Emit(OpCodes.Call, callCancellationTokenNone);
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
                    // use Nil.
                    callType = typeof(Nil);
                    il.Emit(OpCodes.Ldsfld, typeof(Nil).GetField("Default"));
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
                    var mInfo = baseType.GetMethod("WriteMessageWithResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType, typeof(Nil)));
                }
                else
                {
                    var mInfo = baseType.GetMethod("WriteMessageWithResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType, def.MethodInfo.ReturnType.GetGenericArguments()[0]));
                }

                il.Emit(OpCodes.Ret);
            }
        }

        static void DefineMethodsFireAndForget(TypeBuilder typeBuilder, Type interfaceType, FieldInfo clientField, Type parentNestedType, MethodDefinition[] definitions)
        {
            {
                // Task DisposeAsync();
                {
                    var method = typeBuilder.DefineMethod("DisposeAsync", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Newobj, notSupportedException);
                    il.Emit(OpCodes.Throw);
                }
                // Task WaitForDisconnect();
                {
                    var method = typeBuilder.DefineMethod("WaitForDisconnect", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Newobj, notSupportedException);
                    il.Emit(OpCodes.Throw);
                }
                // TSelf FireAndForget();
                {
                    var method = typeBuilder.DefineMethod("FireAndForget", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        interfaceType, Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Newobj, notSupportedException);
                    il.Emit(OpCodes.Throw);
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

                // return client.WriteMessage***<T>(methodId, message);

                // this.client.***
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, clientField);

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
                    // use Nil.
                    callType = typeof(Nil);
                    il.Emit(OpCodes.Ldsfld, typeof(Nil).GetField("Default"));
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
                    var mInfo = parentNestedType.BaseType.GetMethod("WriteMessageAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType));
                }
                else
                {
                    var mInfo = parentNestedType.BaseType.GetMethod("WriteMessageAsyncFireAndForget", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

#endif