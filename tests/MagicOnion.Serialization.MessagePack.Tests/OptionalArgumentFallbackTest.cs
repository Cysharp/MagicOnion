using System.Buffers;
using System.Reflection;
using Grpc.Core;
using MessagePack;

namespace MagicOnion.Serialization.MessagePack.Tests;

public class OptionalArgumentFallbackTest
{
    [Fact]
    public void Deserialize_OmittedArgumentUsesTheMethodDefault()
    {
        var method = typeof(OptionalArgumentFallbackTest).GetMethod(nameof(Method), BindingFlags.Static | BindingFlags.NonPublic)!;
        var serializer = MessagePackMagicOnionSerializerProvider.Default.WithEnableFallback(true).Create(MethodType.Unary, method);
        var payload = new ReadOnlySequence<byte>(new byte[] { 0x91, 0x2a });

        var request = serializer.Deserialize<DynamicArgumentTuple<int, string>>(payload);

        Assert.Equal(42, request.Item1);
        Assert.Equal("fallback", request.Item2);

        var response = new ArrayBufferWriter<byte>();
        serializer.Serialize(response, request.Item1);
        Assert.Equal(42, MessagePackSerializer.Deserialize<int>(response.WrittenSpan));
    }

    static void Method(int id, string label = "fallback") { }
}
