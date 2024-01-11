#if NON_UNITY || ((!ENABLE_IL2CPP || UNITY_EDITOR) && !NET_STANDARD_2_0)

using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Internal.Reflection;
using MagicOnion.Serialization;
using MagicOnion.Server.Hubs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace MagicOnion.Client.DynamicClient
{
#if ENABLE_SAVE_ASSEMBLY
    public
#else
    internal
#endif
        static class DynamicStreamingHubClientAssemblyHolder
    {
        public const string ModuleName = "MagicOnion.Client.StreamingHubClient";

        readonly static DynamicAssembly assembly;
        internal static DynamicAssembly Assembly { get { return assembly; } }

        static DynamicStreamingHubClientAssemblyHolder()
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
        static class DynamicStreamingHubClientBuilder<TStreamingHub, TReceiver>
    {
        public static readonly Type ClientType;
        // static readonly Type ClientFireAndForgetType;

        static readonly ConstructorInfo notSupportedException = typeof(NotSupportedException).GetConstructor(Type.EmptyTypes)!;

        static DynamicStreamingHubClientBuilder()
        {
            var t = typeof(TStreamingHub);
            var ti = t.GetTypeInfo();
            if (!ti.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + ti.Name);
            var asm = DynamicStreamingHubClientAssemblyHolder.Assembly;
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

                var clientField = DefineConstructor(typeBuilder, t, typeof(TReceiver), fireAndForgetClientCtor);
                DefineMethods(typeBuilder, t, typeof(TReceiver), clientField, methodDefinitions);
            }

            ClientType = typeBuilder.CreateTypeInfo()!.AsType();
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
                .Select(x => new MethodDefinition(interfaceType, x, default, default))
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

                var returnTypeNonGenericOrOpenGeneric = item.MethodInfo.ReturnType.IsGenericType ? item.MethodInfo.ReturnType.GetGenericTypeDefinition() : item.MethodInfo.ReturnType;

                if (returnTypeNonGenericOrOpenGeneric != typeof(ValueTask) &&
                    returnTypeNonGenericOrOpenGeneric != typeof(Task) &&
                    returnTypeNonGenericOrOpenGeneric != typeof(ValueTask<>) &&
                    returnTypeNonGenericOrOpenGeneric != typeof(Task<>))
                {
                    throw new Exception($"Invalid definition, TStreamingHub's return type must only be `Task`, `Task<T>`, `ValueTask` or `ValueTask<T>`. {item.MethodInfo.Name}.");
                }

                item.MethodId = methodId;
                if (item.RequestType == null)
                {
                    item.RequestType = MagicOnionMarshallers.CreateRequestType(item.MethodInfo.GetParameters());
                }
            }
        }

        static FieldInfo DefineConstructor(TypeBuilder typeBuilder, Type interfaceType, Type receiverType, ConstructorInfo fireAndForgetClientCtor)
        {
            // .ctor(CallInvoker callInvoker, string host, CallOptions option, IMagicOnionSerializerProvider serializerProvider, IMagicOnionClientLogger logger) :base(...)
            {
                var argTypes = new[] { typeof(CallInvoker), typeof(string), typeof(CallOptions), typeof(IMagicOnionSerializerProvider), typeof(IMagicOnionClientLogger) };
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);
                var il = ctor.GetILGenerator();

                // base(serviceName, callInvoker, host, option, serializerProvider, logger);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, interfaceType.Name);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Ldarg_S, (byte)4);
                il.Emit(OpCodes.Ldarg_S, (byte)5);
                il.Emit(OpCodes.Call, typeBuilder.BaseType!
                    .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First());

                // { this.fireAndForgetClient = new FireAndForgetClient(this); }
                var clientField = typeBuilder.DefineField("fireAndForgetClient", fireAndForgetClientCtor.DeclaringType!, FieldAttributes.Private);
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

        static void DefineMethods(TypeBuilder typeBuilder, Type interfaceType, Type receiverType, FieldInfo clientField, MethodDefinition[] definitions)
        {
            var baseType = typeof(StreamingHubClientBase<,>).MakeGenericType(interfaceType, receiverType);
            var receiverField = baseType.GetField("receiver", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;

            {
                // Task DisposeAsync();
                {
                    var method = typeBuilder.DefineMethod("DisposeAsync", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, baseType.GetMethod("DisposeAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!);
                    il.Emit(OpCodes.Ret);
                }
                // Task WaitForDisconnect();
                {
                    var method = typeBuilder.DefineMethod("WaitForDisconnect", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                        typeof(Task), Type.EmptyTypes);
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, baseType.GetMethod("WaitForDisconnect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!);
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
                        // SetResultForResponse<T>(taskCompletionSource, data);
                        Type responseType;
                        if (item.def.MethodInfo.ReturnType == typeof(Task) || item.def.MethodInfo.ReturnType == typeof(ValueTask))
                        {
                            // Task methods uses TaskCompletionSource<Nil>
                            responseType = typeof(Nil);
                        }
                        else
                        {
                            responseType = item.def.MethodInfo.ReturnType.GetGenericArguments()[0];
                        }

                        il.MarkLabel(item.label);

                        // this.SetResultForResponse<T>(taskCompletionSource, data);
                        il.Emit(OpCodes.Ldarg_0); // this
                        il.Emit(OpCodes.Ldarg_2); // taskCompletionSource
                        il.Emit(OpCodes.Ldarg_3); // data
                        il.Emit(OpCodes.Call, baseType.GetMethod("SetResultForResponse", BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(responseType));

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
                        // var value = Deserialize<DynamicArgumentTuple<int, string>>(data);
                        // receiver.OnMessage(value.Item1, value.Item2);

                        var deserializeMethod = baseType.GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic)!;
                        var parameters = item.def.MethodInfo.GetParameters();
                        if (parameters.Length == 0)
                        {
                            // this.receiver.OnMessage();
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                        }
                        else if (parameters.Length == 1)
                        {
                            // this.receiver.OnMessage(...);
                            il.Emit(OpCodes.Ldarg_0); // this.
                            il.Emit(OpCodes.Ldfld, receiverField); // receiver
                            {
                                // this.Deserialize<T>(data)
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldarg_2); // data
                                il.Emit(OpCodes.Call, deserializeMethod.MakeGenericMethod(parameters[0].ParameterType));
                            }
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                        }
                        else
                        {
                            var deserializeType = BroadcasterHelper.DynamicArgumentTupleTypes[parameters.Length - 2]
                                .MakeGenericType(parameters.Select(x => x.ParameterType).ToArray());
                            var lc = il.DeclareLocal(deserializeType);

                            // var local0 = this.Deserialize<T>(data);
                            {
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldarg_2); // data
                                il.Emit(OpCodes.Call, deserializeMethod.MakeGenericMethod(deserializeType));
                                il.Emit(OpCodes.Stloc, lc);
                            }

                            // this.receiver.OnMessage(local.Item1, local.Item2 ...);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, receiverField);
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                il.Emit(OpCodes.Ldloc, lc);
                                il.Emit(OpCodes.Ldfld, deserializeType.GetField("Item" + (i + 1))!);
                            }
                            il.Emit(OpCodes.Callvirt, item.def.MethodInfo);
                        }
                        // return;
                        il.Emit(OpCodes.Ret);
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

                Type callType;
                if (parameters.Length == 0)
                {
                    // use Nil.
                    callType = typeof(Nil);
                    il.Emit(OpCodes.Ldsfld, typeof(Nil).GetField("Default")!);
                }
                else if (parameters.Length == 1)
                {
                    // already loaded parameter.
                    callType = parameters[0];
                }
                else
                {
                    // call new DynamicArgumentTuple<T>
                    callType = def.RequestType!;
                    il.Emit(OpCodes.Newobj, callType.GetConstructors().First());
                }

                if (def.MethodInfo.ReturnType == typeof(Task) || def.MethodInfo.ReturnType == typeof(ValueTask))
                {
                    var mInfo = baseType.GetMethod("WriteMessageWithResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType, typeof(Nil)));
                }
                else
                {
                    var mInfo = baseType.GetMethod("WriteMessageWithResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
                    il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(callType, def.MethodInfo.ReturnType.GetGenericArguments()[0]));
                }

                // If the return type is `ValueTask`, the task must be wrapped as ValueTask.
                if (def.MethodInfo.ReturnType == typeof(ValueTask))
                {
                    il.Emit(OpCodes.Newobj, typeof(ValueTask).GetConstructor(new [] { typeof(Task) })!);
                }
                else if (def.MethodInfo.ReturnType.IsGenericType && def.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    var returnTypeOfT = def.MethodInfo.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(returnTypeOfT).GetConstructor(new [] { typeof(Task<>).MakeGenericType(returnTypeOfT) })!);
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

                // return client.WriteMessageAsyncFireAndForget<T>(methodId, message);

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

                Type requestType;
                if (parameters.Length == 0)
                {
                    // use Nil.
                    requestType = typeof(Nil);
                    il.Emit(OpCodes.Ldsfld, typeof(Nil).GetField("Default")!);
                }
                else if (parameters.Length == 1)
                {
                    // already loaded parameter.
                    requestType = parameters[0];
                }
                else
                {
                    // call new DynamicArgumentTuple<T>
                    requestType = def.RequestType!;
                    il.Emit(OpCodes.Newobj, requestType.GetConstructors().First());
                }

                Type responseType;
                if (def.MethodInfo.ReturnType == typeof(Task) || def.MethodInfo.ReturnType == typeof(ValueTask))
                {
                    responseType = typeof(Nil);
                }
                else
                {
                    responseType = def.MethodInfo.ReturnType.GetGenericArguments()[0];
                }
                var mInfo = parentNestedType.BaseType!
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single(x => x.Name == "WriteMessageFireAndForgetAsync"); // WriteMessageAsyncFireAndForget<TRequest, TResponse>
                il.Emit(OpCodes.Callvirt, mInfo.MakeGenericMethod(requestType, responseType));

                // If the return type is `ValueTask`, the task must be wrapped as ValueTask.
                if (def.MethodInfo.ReturnType == typeof(ValueTask))
                {
                    il.Emit(OpCodes.Newobj, typeof(ValueTask).GetConstructor(new [] { typeof(Task) })!);
                }
                else if (def.MethodInfo.ReturnType.IsGenericType && def.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    var returnTypeOfT = def.MethodInfo.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(returnTypeOfT).GetConstructor(new [] { typeof(Task<>).MakeGenericType(returnTypeOfT) })!);
                }

                il.Emit(OpCodes.Ret);
            }
        }


        class MethodDefinition
        {
            public string Path => ServiceType.Name + "/" + MethodInfo.Name;

            public Type ServiceType { get; set; }
            public MethodInfo MethodInfo { get; set; }
            public int MethodId { get; set; }

            public Type? RequestType { get; set; }

            public MethodDefinition(Type serviceType, MethodInfo methodInfo, int methodId, Type? requestType)
            {
                ServiceType = serviceType;
                MethodInfo = methodInfo;
                MethodId = methodId;
                RequestType = requestType;
            }
        }
    }
}

#endif
