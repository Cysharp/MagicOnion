using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Client.DynamicClient
{
    internal class DynamicClientBuilder
    {
        protected static class KnownTypes
        {
            public static Type[] ClientConstructorParameters { get; } = new[] { typeof(MagicOnionClientOptions), typeof(MessagePackSerializerOptions) };
            public static Type[] ClientCoreConstructorParameters { get; } = new[] { typeof(MessagePackSerializerOptions) };
        }
    }

    internal class DynamicClientBuilder<T> : DynamicClientBuilder
        where T : IService<T>
    {
        public static Type ClientType { get; } = Build();

        private static Type Build()
        {
            var serviceClientDefinition = ServiceClientDefinition.CreateFromType<T>();
            var buildContext = new ServiceClientBuildContext(serviceClientDefinition);

            EmitServiceClientClass(buildContext);

            return buildContext.ServiceClientType.CreateTypeInfo();
        }

        private class ServiceClientBuildContext
        {
            public ServiceClientBuildContext(ServiceClientDefinition definition)
            {
                Definition = definition;
            }

            public ServiceClientDefinition Definition { get; }

            public TypeBuilder ClientCoreType { get; set; } // {ServiceName}Client+ClientCore
            public ConstructorBuilder ClientCoreConstructor { get; set; } // {ServiceName}Client+ClientCore..ctor

            public TypeBuilder ServiceClientType { get; set; } // {ServiceName}Client
            public ConstructorBuilder ServiceClientConstructor { get; set; } // {ServiceName}Client..ctor
            public FieldBuilder FieldCore { get; set; }

            public Dictionary<string, (FieldBuilder Field, Type MethodInvokerType)> FieldAndMethodInvokerTypeByMethod { get; } = new Dictionary<string, (FieldBuilder Field, Type MethodInvokerType)>();
        }

        private static void EmitServiceClientClass(ServiceClientBuildContext ctx)
        {
            var constructedBaseClientType = typeof(MagicOnionClientBase<>).MakeGenericType(ctx.Definition.ServiceInterfaceType);
            // [Ignore]
            // public class {ServiceName}Client : ClientBase<{ServiceName}>
            // {
            //
            ctx.ServiceClientType = DynamicClientAssemblyHolder.Assembly.DefineType($"{ctx.Definition.ServiceInterfaceType.Name}Client", TypeAttributes.Public | TypeAttributes.Sealed, constructedBaseClientType, new[] { ctx.Definition.ServiceInterfaceType });
            // Set `IgnoreAttribute` to the generated client type. Hides generated-types from building MagicOnion service definitions.
            ctx.ServiceClientType.SetCustomAttribute(new CustomAttributeBuilder(typeof(IgnoreAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>()));
            {
                // class ClientCore { ... }
                EmitClientCore(ctx);
                // private readonly ClientCore core; ...
                EmitFields(ctx);
                // public {ServiceName}Client(ClientOptions options, MessagePackSerializerOptions serializerOptions) { ... } 
                EmitConstructor(ctx);
                // protected override ClientBase<{ServiceName}> Clone(ClientOptions options) => new {ServiceName}Client(options, core);
                EmitClone(ctx, constructedBaseClientType);
                // public UnaryResult<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...) => this.core.MethodName.Invoke(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...)); ...
                EmitServiceMethods(ctx);
            }
            // }
        }

        private static void EmitClone(ServiceClientBuildContext ctx, Type constructedBaseClientType)
        {
            // protected override ClientBase<{ServiceName}> Clone(ClientOptions options) => new {ServiceName}Client(options, core);
            var cloneMethodBuilder = ctx.ServiceClientType.DefineMethod("Clone", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.Final, constructedBaseClientType, new[] { typeof(MagicOnionClientOptions) });
            {
                var il = cloneMethodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1); // options
                il.Emit(OpCodes.Ldarg_0); // this.
                il.Emit(OpCodes.Ldfld, ctx.FieldCore); // core 
                il.Emit(OpCodes.Newobj, ctx.ServiceClientConstructor); // new {ServiceName}Client(options, core);
                il.Emit(OpCodes.Ret);
            }
        }

        private static void EmitConstructor(ServiceClientBuildContext ctx)
        {
            var baseCtor = ctx.ServiceClientType.BaseType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Standard,
                new[] { typeof(MagicOnionClientOptions) },
                Array.Empty<ParameterModifier>()
            );
            // public {ServiceName}Client(MagicOnionClientOptions options, MessagePackSerializerOptions serializerOptions) {
            ctx.ServiceClientConstructor = ctx.ServiceClientType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, KnownTypes.ClientConstructorParameters);
            {
                var il = ctx.ServiceClientConstructor.GetILGenerator();
                // base(options);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, baseCtor);
                // this.core = new ClientCore(serializerOptions);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Newobj, ctx.ClientCoreConstructor);
                il.Emit(OpCodes.Stfld, ctx.FieldCore);
                il.Emit(OpCodes.Ret);
            }
            // }
        }

        private static void EmitFields(ServiceClientBuildContext ctx)
        {
            // private readonly ClientCore core;
            ctx.FieldCore = ctx.ServiceClientType.DefineField("core", ctx.ClientCoreType, FieldAttributes.Private);
        }

        private static void EmitServiceMethods(ServiceClientBuildContext ctx)
        {
            // Implements
            // public UnaryResult<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...)
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
            // public UnaryResult<TResponse> MethodName(TRequest request)
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName", request);
            // public UnaryResult<TResponse> MethodName()
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName", Nil.Default);
            // public ServerStreamingResult<TRequest, TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...)
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
            // public ClientStreamingResult<TRequest, TResponse> MethodName()
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName");
            // public DuplexStreamingResult<TRequest, TResponse> MethodName()
            //     => this.core.MethodName.Invoke(this, "ServiceName/MethodName");
            foreach (var method in ctx.Definition.Methods)
            {
                var methodInvokerInvokeMethod = ctx.FieldAndMethodInvokerTypeByMethod[method.MethodName].MethodInvokerType.GetMethod("Invoke");
                var methodBuilder = ctx.ServiceClientType.DefineMethod(method.MethodName, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual, methodInvokerInvokeMethod.ReturnType, method.ParameterTypes.ToArray());
                var il = methodBuilder.GetILGenerator();

                // return this.core.{Method}(
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ctx.FieldCore);
                il.Emit(OpCodes.Ldfld, ctx.FieldAndMethodInvokerTypeByMethod[method.MethodName].Field);
                //     this,
                il.Emit(OpCodes.Ldarg_0);
                //     method.Path,
                il.Emit(OpCodes.Ldstr, method.Path);

                if (method.MethodType == MethodType.Unary || method.MethodType == MethodType.ServerStreaming)
                {
                    if (method.ParameterTypes.Count > 0)
                    {
                        if (method.ParameterTypes.Count == 1)
                        {
                            // arg1
                            il.Emit(OpCodes.Ldarg_1);
                        }
                        else
                        {
                            // new DynamicArgumentTuple(arg1, arg2, ...)
                            for (var i = 0; i < method.ParameterTypes.Count; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        il.Emit(OpCodes.Ldarg_1);
                                        break;
                                    case 1:
                                        il.Emit(OpCodes.Ldarg_2);
                                        break;
                                    case 2:
                                        il.Emit(OpCodes.Ldarg_3);
                                        break;
                                    default:
                                        il.Emit(OpCodes.Ldarg, i + 1);
                                        break;
                                }
                            }
                            il.Emit(OpCodes.Newobj, method.RequestType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, method.ParameterTypes.ToArray(), Array.Empty<ParameterModifier>()));
                        }
                    }
                    else if (method.ParameterTypes.Count == 0)
                    {
                        // Nil.Default
                        il.Emit(OpCodes.Ldsfld, typeof(Nil).GetField("Default", BindingFlags.Public | BindingFlags.Static));
                    }
                }
                else
                {
                    // Invoker for ClientStreaming, DuplexStreaming has no request parameter.
                }

                // );
                il.Emit(OpCodes.Callvirt, methodInvokerInvokeMethod);
                il.Emit(OpCodes.Ret);
            }
        }

        private static void EmitClientCore(ServiceClientBuildContext ctx)
        {
            /*
             * class ClientCore
             * {
             *     // UnaryResult<string> HelloAsync(string name, int age);
             *     public UnaryMethodRawInvoker<DynamicArgumentTuple<string, int>, string> HelloAsync;
             *
             *     public ClientCore(MessagePackSerializer options)
             *     {
             *         this.HelloAsync = MethodInvoker.UnaryMethodRawInvoker<DynamicArgumentTuple<string, int>, string>("IGreeterService", "HelloAsync", options);
             *     }
             * }
             */

            // class ClientCore {
            ctx.ClientCoreType = ctx.ServiceClientType.DefineNestedType("ClientCore");
            {
                // public {Unary,ServerStreaming,ClientStreaming,DuplexStreaming}MethodRawInvoker<TRequest, TResponse> MethodName;
                foreach (var method in ctx.Definition.Methods)
                {
                    var methodInvokerType = MethodRawInvokerTypes.GetMethodRawInvokerType(method.MethodType, method.RequestType, method.ResponseType);
                    var field = ctx.ClientCoreType.DefineField(method.MethodName, methodInvokerType, FieldAttributes.Public);
                    ctx.FieldAndMethodInvokerTypeByMethod[method.MethodName] = (field, methodInvokerType);
                }

                // public ClientCore(MessagePackSerializerOptions serializerOptions) {
                ctx.ClientCoreConstructor = ctx.ClientCoreType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, KnownTypes.ClientCoreConstructorParameters);
                {
                    var il = ctx.ClientCoreConstructor.GetILGenerator();

                    // MethodName = MethodRawInvoker.Create_XXXType_XXXType<TRequest, TResponse>(ServiceName, MethodName, serializerOptions);
                    foreach (var method in ctx.Definition.Methods)
                    {
                        il.Emit(OpCodes.Ldarg_0);

                        var (field, _) = ctx.FieldAndMethodInvokerTypeByMethod[method.MethodName];
                        var methodInvokerType = MethodRawInvokerTypes.GetMethodRawInvokerCreateMethod(method.MethodType, method.RequestType, method.ResponseType);
                        // UnaryMethodRawInvoker<TRequest..., TResponse>.Create_XXXType_XXXType(
                        il.Emit(OpCodes.Ldstr, method.ServiceName); // serviceName,
                        il.Emit(OpCodes.Ldstr, method.MethodName); // methodName,
                        il.Emit(OpCodes.Ldarg_1); // serializerOptions
                        il.Emit(OpCodes.Call, methodInvokerType);
                        // );

                        // = <stack>;
                        il.Emit(OpCodes.Stfld, field);
                    }
                    il.Emit(OpCodes.Ret);
                }
                // }
            }
            // }
            _ = ctx.ClientCoreType.CreateTypeInfo(); // Build
        }
    }
}
