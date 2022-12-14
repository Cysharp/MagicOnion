using System.CodeDom.Compiler;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.CodeGen.Extensions;
using MagicOnion.Generator.Internal;

namespace MagicOnion.Generator.CodeGen;

public class StaticMagicOnionClientGenerator
{
    class ServiceClientBuildContext
    {
        public ServiceClientBuildContext(MagicOnionServiceInfo service, IndentedTextWriter textWriter)
        {
            Service = service;
            TextWriter = textWriter;
        }

        public MagicOnionServiceInfo Service { get; }

        public IndentedTextWriter TextWriter { get; }
    }

    public static string Build(IEnumerable<MagicOnionServiceInfo> services)
    {
        var baseWriter = new StringWriter();
        var textWriter = new IndentedTextWriter(baseWriter);

        EmitHeader(textWriter);

        foreach (var serviceInfo in services)
        {
            var buildContext = new ServiceClientBuildContext(serviceInfo, textWriter);

            using (textWriter.IfDirective(serviceInfo.IfDirectiveCondition)) // #if ...
            {
                EmitPreamble(buildContext);
                EmitServiceClientClass(buildContext);
                EmitPostscript(buildContext);
            } // #endif
        }

        return baseWriter.ToString();
    }

    static void EmitHeader(IndentedTextWriter textWriter)
    {
        textWriter.WriteLines("""
        #pragma warning disable 618
        #pragma warning disable 612
        #pragma warning disable 414
        #pragma warning disable 219
        #pragma warning disable 168
        
        // NOTE: Disable warnings for nullable reference types.
        // `#nullable disable` causes compile error on old C# compilers (-7.3)
        #pragma warning disable 8603 // Possible null reference return.
        #pragma warning disable 8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
        #pragma warning disable 8625 // Cannot convert null literal to non-nullable reference type.
        """);
        textWriter.WriteLine();
    }

    static void EmitPreamble(ServiceClientBuildContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        namespace {{ctx.Service.ServiceType.Namespace}}
        {
            using global::System;
            using global::Grpc.Core;
            using global::MagicOnion;
            using global::MagicOnion.Client;
            using global::MessagePack;
        """);
        ctx.TextWriter.Indent++;
        ctx.TextWriter.WriteLine();
    }

    static void EmitPostscript(ServiceClientBuildContext ctx)
    {
        ctx.TextWriter.Indent--;
        ctx.TextWriter.WriteLine("}");
        ctx.TextWriter.WriteLine();
    }

    static void EmitServiceClientClass(ServiceClientBuildContext ctx)
    {
        // [Ignore]
        // public class {ServiceName}Client : MagicOnionClientBase<{ServiceName}>, {ServiceName}
        // {
        //
        ctx.TextWriter.WriteLines($$"""
        [global::MagicOnion.Ignore]
        public class {{ctx.Service.GetClientName()}} : global::MagicOnion.Client.MagicOnionClientBase<{{ctx.Service.ServiceType.FullName}}>, {{ctx.Service.ServiceType.FullName}}
        {
        """);
        using (ctx.TextWriter.BeginIndent())
        {
            // class ClientCore { ... }
            EmitClientCore(ctx);
            // private readonly ClientCore core; ...
            EmitFields(ctx);
            // public {ServiceName}Client(MagicOnionClientOptions options, IMagicOnionMessageSerializerProvider messageSerializer) : base(options) { ... }
            // private {ServiceName}Client(MagicOnionClientOptions options, ClientCore core) : base(options) { ... }
            EmitConstructor(ctx);
            // protected override ClientBase<{ServiceName}> Clone(MagicOnionClientOptions options) => new {ServiceName}Client(options, core);
            EmitClone(ctx);
            // public {MethodType}Result<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...) => this.core.MethodName.Invoke{MethodType}(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...)); ...
            EmitServiceMethods(ctx);
        }
        ctx.TextWriter.WriteLine("}");
        // }
    }

    static void EmitClone(ServiceClientBuildContext ctx)
    {
        ctx.TextWriter.WriteLines($"""
        protected override global::MagicOnion.Client.MagicOnionClientBase<{ctx.Service.ServiceType.Name}> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)
            => new {ctx.Service.GetClientName()}(options, core);
        """);
        ctx.TextWriter.WriteLine();
    }

    static void EmitConstructor(ServiceClientBuildContext ctx)
    {
        ctx.TextWriter.WriteLines($$"""
        public {{ctx.Service.GetClientName()}}(global::MagicOnion.Client.MagicOnionClientOptions options, global::MagicOnion.Serialization.IMagicOnionMessageSerializerProvider messageSerializer) : base(options)
        {
            this.core = new ClientCore(messageSerializer);
        }

        private {{ctx.Service.GetClientName()}}(MagicOnionClientOptions options, ClientCore core) : base(options)
        {
            this.core = core;
        }
        """);
        ctx.TextWriter.WriteLine();
    }

    static void EmitFields(ServiceClientBuildContext ctx)
    {
        // private readonly ClientCore core;
        ctx.TextWriter.WriteLine("readonly ClientCore core;");
        ctx.TextWriter.WriteLine();
    }

    static void EmitServiceMethods(ServiceClientBuildContext ctx)
    {
        // Implements
        // public UnaryResult<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...)
        //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
        // public UnaryResult<TResponse> MethodName(TRequest request)
        //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", request);
        // public UnaryResult<TResponse> MethodName()
        //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", Nil.Default);
        // public UnaryResult MethodName()
        //     => this.core.MethodName.InvokeUnaryNonGeneric(this, "ServiceName/MethodName", Nil.Default);
        // public Task<ServerStreamingResult<TRequest, TResponse>> MethodName(TArg1 arg1, TArg2 arg2, ...)
        //     => this.core.MethodName.InvokeServerStreaming(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
        // public Task<ClientStreamingResult<TRequest, TResponse>> MethodName()
        //     => this.core.MethodName.InvokeClientStreaming(this, "ServiceName/MethodName");
        // public Task<DuplexStreamingResult<TRequest, TResponse>> MethodName()
        //     => this.core.MethodName.InvokeDuplexStreaming(this, "ServiceName/MethodName");
        foreach (var method in ctx.Service.Methods)
        {
            using (ctx.TextWriter.IfDirective(method.IfDirectiveCondition)) // #if ...
            {
                var invokeRequestParameters = method.Parameters.Count switch
                {
                    // Invoker for ClientStreaming, DuplexStreaming method has no request parameter.
                    _ when (method.MethodType != MethodType.Unary && method.MethodType != MethodType.ServerStreaming) => $"",
                    // Nil.Default
                    0 => $", global::MessagePack.Nil.Default",
                    // arg0
                    1 => $", {method.Parameters[0].Name}",
                    // new DynamicArgumentTuple(arg1, arg2, ...)
                    _ => $", {method.Parameters.ToNewDynamicArgumentTuple()}",
                };
                var hasNonGenericUnaryResult = method.MethodReturnType == MagicOnionTypeInfo.KnownTypes.MagicOnion_UnaryResult;

                ctx.TextWriter.WriteLines($"""
                public {method.MethodReturnType.FullName} {method.MethodName}({method.Parameters.ToMethodSignaturize()})
                    => this.core.{method.MethodName}.Invoke{method.MethodType}{(hasNonGenericUnaryResult ? "NonGeneric" : "")}(this, "{method.Path}"{invokeRequestParameters});
                """);
            } // #endif
        }
    }

    static void EmitClientCore(ServiceClientBuildContext ctx)
    {
        /*
         * class ClientCore
         * {
         *     // UnaryResult<string> HelloAsync(string name, int age);
         *     public UnaryMethodRawInvoker<DynamicArgumentTuple<string, int>, string> HelloAsync;
         *
         *     public ClientCore(IMagicOnionMessageSerializerProvider messageSerializer)
         *     {
         *         this.HelloAsync = UnaryMethodRawInvoker.Create_ValueType_RefType<DynamicArgumentTuple<string, int>, string>("IGreeterService", "HelloAsync", messageSerializer);
         *     }
         * }
         */

        // class ClientCore {
        ctx.TextWriter.WriteLine("class ClientCore");
        ctx.TextWriter.WriteLine("{");
        using (ctx.TextWriter.BeginIndent())
        {
            // public RawMethodInvoker<TRequest, TResponse> MethodName;
            foreach (var method in ctx.Service.Methods)
            {
                using (ctx.TextWriter.IfDirective(method.IfDirectiveCondition)) // #if ...
                {
                    ctx.TextWriter.WriteLine($"public global::MagicOnion.Client.Internal.RawMethodInvoker<{method.RequestType.FullName}, {method.ResponseType.FullName}> {method.MethodName};");
                } // #endif
            }

            // public ClientCore(IMagicOnionMessageSerializerProvider messageSerializer) {
            ctx.TextWriter.WriteLine("public ClientCore(global::MagicOnion.Serialization.IMagicOnionMessageSerializerProvider messageSerializer)");
            ctx.TextWriter.WriteLine("{");
            using (ctx.TextWriter.BeginIndent())
            {
                // MethodName = RawMethodInvoker.Create_XXXType_XXXType<TRequest, TResponse>(MethodType, ServiceName, MethodName, messageSerializer);
                foreach (var method in ctx.Service.Methods)
                {
                    using (ctx.TextWriter.IfDirective(method.IfDirectiveCondition)) // #if ...
                    {
                        var createMethodVariant = $"{(method.RequestType.IsValueType ? "Value" : "Ref")}Type_{(method.ResponseType.IsValueType ? "Value" : "Ref")}Type";
                        ctx.TextWriter.WriteLine($"this.{method.MethodName} = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_{createMethodVariant}<{method.RequestType.FullName}, {method.ResponseType.FullName}>(global::Grpc.Core.MethodType.{method.MethodType}, \"{method.ServiceName}\", \"{method.MethodName}\", messageSerializer);");
                    } // #endif
                }
            }
            ctx.TextWriter.WriteLine("}");
            // }
        }
        // }
        ctx.TextWriter.WriteLine("}");
        ctx.TextWriter.WriteLine();
    }
}
