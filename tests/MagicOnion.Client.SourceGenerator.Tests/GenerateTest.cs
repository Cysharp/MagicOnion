using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateTest
{
    [Fact]
    public async Task Test1()
    {
        var source = """
        using MagicOnion;

        namespace MyApplication1;

        public interface IGreeterService : IService<IGreeterService>
        {
            UnaryResult<string> HelloAsync(string name, int age);
            UnaryResult PingAsync();
            UnaryResult<bool> CanGreetAsync();
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task ImplicitUsings_PropertyGroup_Enable()
    {
        var sources = new []
        {
            ("Usings.cs",
                """
                // ImplicitUsings: Microsoft.NET.Sdk
                global using global::System;
                global using global::System.Collections.Generic;
                global using global::System.IO;
                global using global::System.Linq;
                global using global::System.Net.Http;
                global using global::System.Threading;
                global using global::System.Threading.Tasks;
                """),
            ("IMyService.cs",
                """
                using MagicOnion;
                using MessagePack;
                
                namespace MyNamespace;
                
                public interface IMyService : IService<IMyService>
                {
                    UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
                }
                """
            ),
        };
        await MagicOnionSourceGeneratorVerifier.RunAsync(sources);
    }

    //[Fact]
    //public async Task ImplicitUsings_PropertyGroup_Disable()
    //{
    //    var sources = new []
    //    {
    //        ("IMyService.cs",
    //            """
    //            using MagicOnion;
    //            using MessagePack;
    //            
    //            namespace MyNamespace;
    //            
    //            public interface IMyService : IService<IMyService>
    //            {
    //                UnaryResult<Nil> A(Int32 arg0, IReadOnlyList<int> arg1, FileMode arg2, ILookup<string, string> arg3, ClientCertificateOption arg4, ApartmentState arg5, TaskCreationOptions arg6);
    //            }
    //            """
    //        ),
    //    };
    //    await MagicOnionSourceGeneratorVerifier.RunAsync(sources);
    //}
}
