using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator
{
    public class StaticClientBuilder
    {
        private class ServiceClientBuildContext
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

                EmitPreamble(buildContext);
                EmitServiceClientClass(buildContext);
                EmitPostscript(buildContext);
            }

            return baseWriter.ToString();
        }

        private static void EmitHeader(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine("#pragma warning disable 618");
            textWriter.WriteLine("#pragma warning disable 612");
            textWriter.WriteLine("#pragma warning disable 414");
            textWriter.WriteLine("#pragma warning disable 219");
            textWriter.WriteLine("#pragma warning disable 168");
            textWriter.WriteLine();
        }
        
        private static void EmitPreamble(ServiceClientBuildContext ctx)
        {
            ctx.TextWriter.WriteLine($"namespace {ctx.Service.ServiceType.Namespace}");
            ctx.TextWriter.WriteLine("{");
            ctx.TextWriter.Indent++;
            ctx.TextWriter.WriteLine("using global::System;");
            ctx.TextWriter.WriteLine("using global::Grpc.Core;");
            ctx.TextWriter.WriteLine("using global::MagicOnion;");
            ctx.TextWriter.WriteLine("using global::MagicOnion.Client;");
            ctx.TextWriter.WriteLine("using global::MessagePack;");
            ctx.TextWriter.WriteLine();
        }

        private static void EmitPostscript(ServiceClientBuildContext ctx)
        {
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine("}");
            ctx.TextWriter.WriteLine();
        }

        private static void EmitServiceClientClass(ServiceClientBuildContext ctx)
        {
            // [Ignore]
            // public class {ServiceName}Client : MagicOnionClientBase<{ServiceName}>, {ServiceName}
            // {
            //
            ctx.TextWriter.WriteLine("[global::MagicOnion.Ignore]");
            ctx.TextWriter.WriteLine($"public class {ctx.Service.GetClientName()} : global::MagicOnion.Client.MagicOnionClientBase<{ctx.Service.ServiceType.FullName}>, {ctx.Service.ServiceType.FullName}");
            ctx.TextWriter.WriteLine("{");
            ctx.TextWriter.Indent++;
            {
                // class ClientCore { ... }
                EmitClientCore(ctx);
                // private readonly ClientCore core; ...
                EmitFields(ctx);
                // public {ServiceName}Client(MagicOnionClientOptions options, MessagePackSerializerOptions serializerOptions) : base(options) { ... } 
                // private {ServiceName}Client(MagicOnionClientOptions options, ClientCore core) : base(options) { ... } 
                EmitConstructor(ctx);
                // protected override ClientBase<{ServiceName}> Clone(MagicOnionClientOptions options) => new {ServiceName}Client(options, core);
                EmitClone(ctx);
                // public {MethodType}Result<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...) => this.core.MethodName.Invoke{MethodType}(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...)); ...
                EmitServiceMethods(ctx);
            }
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine("}");
            // }
        }

        private static void EmitClone(ServiceClientBuildContext ctx)
        {
            // protected override MagicOnionClientBase<{ServiceName}> Clone(MagicOnionClientOptions options) => new {ServiceName}Client(options, core);
            ctx.TextWriter.WriteLine($"protected override global::MagicOnion.Client.MagicOnionClientBase<{ctx.Service.ServiceType.Name}> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)");
            ctx.TextWriter.Indent++;
            ctx.TextWriter.WriteLine($"=> new {ctx.Service.GetClientName()}(options, core);");
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine();
        }

        private static void EmitConstructor(ServiceClientBuildContext ctx)
        {
            // public {ServiceName}Client(MagicOnionClientOptions options, MessagePackSerializerOptions serializerOptions) : base(options)
            // {
            ctx.TextWriter.WriteLine($"public {ctx.Service.GetClientName()}(global::MagicOnion.Client.MagicOnionClientOptions options, global::MessagePack.MessagePackSerializerOptions serializerOptions) : base(options)");
            ctx.TextWriter.WriteLine("{");
            ctx.TextWriter.Indent++;
            {
                // this.core = new ClientCore(serializerOptions);
                ctx.TextWriter.WriteLine("this.core = new ClientCore(serializerOptions);");
            }
            // }
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine("}");
            ctx.TextWriter.WriteLine();

            // private {ServiceName}Client(MagicOnionClientOptions options, ClientCore serializerOptions) : base(options)
            // {
            ctx.TextWriter.WriteLine($"private {ctx.Service.GetClientName()}(global::MagicOnion.Client.MagicOnionClientOptions options, ClientCore core) : base(options)");
            ctx.TextWriter.WriteLine("{");
            ctx.TextWriter.Indent++;
            {
                // this.core = new ClientCore(serializerOptions);
                ctx.TextWriter.WriteLine("this.core = core;");
            }
            // }
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine("}");
            ctx.TextWriter.WriteLine();
        }

        private static void EmitFields(ServiceClientBuildContext ctx)
        {
            // private readonly ClientCore core;
            ctx.TextWriter.WriteLine("readonly ClientCore core;");
            ctx.TextWriter.WriteLine();
        }

        private static void EmitServiceMethods(ServiceClientBuildContext ctx)
        {
            // Implements
            // public UnaryResult<TResponse> MethodName(TArg1 arg1, TArg2 arg2, ...)
            //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
            // public UnaryResult<TResponse> MethodName(TRequest request)
            //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", request);
            // public UnaryResult<TResponse> MethodName()
            //     => this.core.MethodName.InvokeUnary(this, "ServiceName/MethodName", Nil.Default);
            // public Task<ServerStreamingResult<TRequest, TResponse>> MethodName(TArg1 arg1, TArg2 arg2, ...)
            //     => this.core.MethodName.InvokeServerStreaming(this, "ServiceName/MethodName", new DynamicArgumentTuple<T1, T2, ...>(arg1, arg2, ...));
            // public Task<ClientStreamingResult<TRequest, TResponse>> MethodName()
            //     => this.core.MethodName.InvokeClientStreaming(this, "ServiceName/MethodName");
            // public Task<DuplexStreamingResult<TRequest, TResponse>> MethodName()
            //     => this.core.MethodName.InvokeDuplexStreaming(this, "ServiceName/MethodName");
            foreach (var method in ctx.Service.Methods)
            {
                ctx.TextWriter.WriteLine($"public {method.MethodReturnType.FullName} {method.MethodName}({string.Join(", ", method.Parameters.Select((x, i) => $"{x.Type.FullName} {x.Name}"))})");
                ctx.TextWriter.Indent++;
                ctx.TextWriter.Write($"=> this.core.{method.MethodName}.Invoke{method.MethodType}(this, \"{method.Path}\"");
                if (method.MethodType == MethodType.Unary || method.MethodType == MethodType.ServerStreaming)
                {
                    if (method.Parameters.Count > 0)
                    {
                        if (method.Parameters.Count == 1)
                        {
                            // arg1
                            ctx.TextWriter.Write($", {method.Parameters[0].Name}");
                        }
                        else
                        {
                            // new DynamicArgumentTuple(arg1, arg2, ...)
                            ctx.TextWriter.Write($", new global::MagicOnion.DynamicArgumentTuple<{string.Join(", ", method.Parameters.Select((x, i) => $"{x.Type.FullName}"))}>({string.Join(", ", method.Parameters.Select((x, i) => x.Name))})");
                        }
                    }
                    else if (method.Parameters.Count == 0)
                    {
                        // Nil.Default
                        ctx.TextWriter.Write(", global::MessagePack.Nil.Default");
                    }
                }
                else
                {
                    // Invoker for ClientStreaming, DuplexStreaming has no request parameter.
                }

                // );
                ctx.TextWriter.WriteLine(");");
                ctx.TextWriter.Indent--;
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
             *         this.HelloAsync = UnaryMethodRawInvoker.Create_ValueType_RefType<DynamicArgumentTuple<string, int>, string>("IGreeterService", "HelloAsync", options);
             *     }
             * }
             */

            // class ClientCore {
            ctx.TextWriter.WriteLine("class ClientCore");
            ctx.TextWriter.WriteLine("{");
            ctx.TextWriter.Indent++;
            {
                // public RawMethodInvoker<TRequest, TResponse> MethodName;
                foreach (var method in ctx.Service.Methods)
                {
                    ctx.TextWriter.WriteLine($"public global::MagicOnion.Client.Internal.RawMethodInvoker<{method.RequestType.FullName}, {method.ResponseType.FullName}> {method.MethodName};");
                }

                // public ClientCore(MessagePackSerializerOptions serializerOptions) {
                ctx.TextWriter.WriteLine("public ClientCore(global::MessagePack.MessagePackSerializerOptions serializerOptions)");
                ctx.TextWriter.WriteLine("{");
                ctx.TextWriter.Indent++;
                {
                    // MethodName = RawMethodInvoker.Create_XXXType_XXXType<TRequest, TResponse>(MethodType, ServiceName, MethodName, serializerOptions);
                    foreach (var method in ctx.Service.Methods)
                    {
                        var createMethodVariant = $"{(method.RequestType.IsValueType ? "Value" : "Ref")}Type_{(method.ResponseType.IsValueType ? "Value" : "Ref")}Type";
                        ctx.TextWriter.WriteLine($"this.{method.MethodName} = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_{createMethodVariant}<{method.RequestType.FullName}, {method.ResponseType.FullName}>(global::Grpc.Core.MethodType.{method.MethodType}, \"{method.ServiceName}\", \"{method.MethodName}\", serializerOptions);");
                    }
                }
                ctx.TextWriter.Indent--;
                ctx.TextWriter.WriteLine("}");
                // }
            }
            // }
            ctx.TextWriter.Indent--;
            ctx.TextWriter.WriteLine("}");
            ctx.TextWriter.WriteLine();
        }
    }
}
