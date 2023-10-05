using MagicOnion.Client.SourceGenerator.Tests.Verifiers;

namespace MagicOnion.Client.SourceGenerator.Tests;

public class GenerateStreamingHubTest
{
    [Fact]
    public async Task Complex()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void OnMessage();
                void OnMessage2(MyObject a);
                void OnMessage3(MyObject a, string b, int c);
        
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A();
                Task B(MyObject a);
                Task C(MyObject a, string b);
                Task D(MyObject a, string b, int c);
                Task<int> E(MyObject a, string b, int c);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Parameter_Zero()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void OnMessage();
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Parameter_One()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void OnMessage(MyObject arg0);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task HubReceiver_Parameter_Many()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                void OnMessage(MyObject arg0, int arg1, string arg2);
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_Task()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_TaskOfT()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task<MyObject> A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ValueTask()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                ValueTask A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Return_ValueTaskOfT()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                ValueTask<MyObject> A(MyObject a);
            }
        
            [MessagePackObject]
            public class MyObject
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Invalid_Return_Void()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                void A();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Invalid_HubReceiver_ReturnsNotVoid()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver
            {
                Task B();
            }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameter_Zero()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A();
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameter_One()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;
        
        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(string arg0);
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }

    [Fact]
    public async Task Parameter_Many()
    {
        var source = """
        using System;
        using System.Threading.Tasks;
        using MessagePack;
        using MagicOnion;

        namespace TempProject
        {
            public interface IMyHubReceiver { }
            public interface IMyHub : IStreamingHub<IMyHub, IMyHubReceiver>
            {
                Task A(string arg0, int arg1, bool arg2);
            }
        }
        """;

        await MagicOnionSourceGeneratorVerifier.RunAsync(source);
    }
}