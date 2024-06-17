using System.Buffers;
using Cysharp.Runtime.Multicast.Remoting;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Hubs;

internal class MagicOnionRemoteSerializer : IRemoteSerializer
{
    readonly IMagicOnionSerializer serializer;

    public MagicOnionRemoteSerializer(IOptions<MagicOnionOptions> options)
    {
        this.serializer = options.Value.MessageSerializer.Create(MethodType.DuplexStreaming, null);
    }

    public void SerializeInvocation(IBufferWriter<byte> bufferWriter, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
    {
        if (ctx.MessageId is { } messageId)
        {
            StreamingHubMessageWriter.WriteClientResultRequestMessage(bufferWriter, ctx.MethodId, messageId, Nil.Default, serializer);
        }
        else
        {
            StreamingHubMessageWriter.WriteBroadcastMessage(bufferWriter, ctx.MethodId, Nil.Default, serializer);
        }
    }

    public void SerializeInvocation<T>(IBufferWriter<byte> bufferWriter, T value, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
    {
        if (ctx.MessageId is { } messageId)
        {
            StreamingHubMessageWriter.WriteClientResultRequestMessage(bufferWriter, ctx.MethodId, messageId, value, serializer);
        }
        else
        {
            StreamingHubMessageWriter.WriteBroadcastMessage(bufferWriter, ctx.MethodId, value, serializer);
        }
    }

    public void SerializeInvocation<T1, T2>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2>(arg1, arg2), ctx);
    public void SerializeInvocation<T1, T2, T3>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3>(arg1, arg2, arg3), ctx);
    public void SerializeInvocation<T1, T2, T3, T4>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4>(arg1, arg2, arg3, arg4), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5>(arg1, arg2, arg3, arg4, arg5), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(arg1, arg2, arg3, arg4, arg5, arg6), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(arg1, arg2, arg3, arg4, arg5, arg6, arg7), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14), ctx);
    public void SerializeInvocation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(IBufferWriter<byte> writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => SerializeInvocation(writer, new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15), ctx);

    public T DeserializeResult<T>(ReadOnlySequence<byte> data, in Cysharp.Runtime.Multicast.Remoting.SerializationContext ctx)
        => serializer.Deserialize<T>(data);
}
