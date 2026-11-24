using System.Buffers;
using System.Runtime.CompilerServices;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MessagePack;

namespace MagicOnion.Client.Tests;

public class GrpcMethodHelperPayloadLengthTest
{
    static readonly IMagicOnionSerializer serializer = MagicOnionSerializerProvider.Default.Create(MethodType.Unary, null);

    [Fact]
    public void RefType_SetsPayloadLengthBeforeGettingBufferWriter()
    {
        // Arrange
        var value = string.Join(",", Enumerable.Range(0, 10000));
        var method = GrpcMethodHelper.CreateMethod<string, string, string, string>(MethodType.Unary, "IService", "Method", serializer);
        var context = new RecordingSerializationContext();

        // Act
        method.ResponseMarshaller.ContextualSerializer(value, context);

        // Assert
        Assert.Equal(["SetPayloadLength", "GetBufferWriter", "Complete"], context.Calls);
        Assert.Equal(context.Written.Length, context.PayloadLength);
        Assert.Equal(MessagePackSerializer.Serialize(value), context.Written.ToArray());
    }

    [Fact]
    public void ValueType_SetsPayloadLengthBeforeGettingBufferWriter()
    {
        // Arrange
        var method = GrpcMethodHelper.CreateMethod<int, long, Box<int>, Box<long>>(MethodType.Unary, "IService", "Method", serializer);
        var context = new RecordingSerializationContext();

        // Act
        method.ResponseMarshaller.ContextualSerializer(Box.Create(long.MaxValue), context);

        // Assert
        Assert.Equal(["SetPayloadLength", "GetBufferWriter", "Complete"], context.Calls);
        Assert.Equal(context.Written.Length, context.PayloadLength);
        Assert.Equal(MessagePackSerializer.Serialize(long.MaxValue), context.Written.ToArray());
    }

    [Fact]
    public void RawBytes_SetsPayloadLengthBeforeGettingBufferWriter()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var method = GrpcMethodHelper.CreateMethod<string, string, string, string>(MethodType.Unary, "IService", "Method", serializer);
        var context = new RecordingSerializationContext();
        var rawBytesBox = new RawBytesBox(bytes);

        // Act
        // NOTE: The server disguises RawBytesBox as a response object in the same way.
        method.ResponseMarshaller.ContextualSerializer(Unsafe.As<RawBytesBox, string>(ref rawBytesBox), context);

        // Assert
        Assert.Equal(["SetPayloadLength", "GetBufferWriter", "Complete"], context.Calls);
        Assert.Equal(bytes.Length, context.PayloadLength);
        Assert.Equal(bytes, context.Written.ToArray());
    }

    [Fact]
    public void ParameterlessRequest_SetsPayloadLengthBeforeGettingBufferWriter()
    {
        // Arrange
        var method = GrpcMethodHelper.CreateMethod<string, string>(MethodType.Unary, "IService", "Method", serializer);
        var context = new RecordingSerializationContext();

        // Act
        method.RequestMarshaller.ContextualSerializer(Box.Create(Nil.Default), context);

        // Assert
        Assert.Equal(["SetPayloadLength", "GetBufferWriter", "Complete"], context.Calls);
        Assert.Equal(1, context.PayloadLength);
        Assert.Equal([MessagePackCode.Nil], context.Written.ToArray());
    }

    [Fact]
    public void Serialize_CanBeCalledRepeatedlyWithDifferentSizes()
    {
        // Arrange
        var method = GrpcMethodHelper.CreateMethod<string, string, string, string>(MethodType.Unary, "IService", "Method", serializer);

        foreach (var length in new[] { 0, 1, 100, 100_000, 10, 1_000_000, 5 })
        {
            var value = new string('x', length);
            var context = new RecordingSerializationContext();

            // Act
            method.ResponseMarshaller.ContextualSerializer(value, context);

            // Assert
            Assert.Equal(context.Written.Length, context.PayloadLength);
            Assert.Equal(value, MessagePackSerializer.Deserialize<string>(context.Written));
        }
    }

    // Behaves like grpc-dotnet: the payload length must be set before the buffer writer is requested.
    sealed class RecordingSerializationContext : SerializationContext
    {
        readonly ArrayBufferWriter<byte> writer = new();

        public List<string> Calls { get; } = new();
        public int? PayloadLength { get; private set; }
        public ReadOnlyMemory<byte> Written => writer.WrittenMemory;

        public override void SetPayloadLength(int payloadLength)
        {
            if (Calls.Contains("GetBufferWriter")) throw new InvalidOperationException("The payload length must be set before getting the buffer writer.");
            Calls.Add("SetPayloadLength");
            PayloadLength = payloadLength;
        }

        public override IBufferWriter<byte> GetBufferWriter()
        {
            if (!Calls.Contains("GetBufferWriter")) Calls.Add("GetBufferWriter");
            return writer;
        }

        public override void Complete()
            => Calls.Add("Complete");

        public override void Complete(byte[] payload)
            => throw new NotSupportedException();
    }
}
