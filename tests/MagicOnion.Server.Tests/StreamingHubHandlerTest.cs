using System.Buffers;
using Grpc.Core;
using MagicOnion.Internal;
using MagicOnion.Serialization;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Hubs;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MagicOnion.Server.Tests;

public class StreamingHubHandlerTest
{
    [Fact]
    public async Task Parameterless_Returns_Task()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_Task))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_Task),
            static (instance, context, _) => instance.Method_Parameterless_Returns_Task());
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(Nil.Default), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_Task) + " called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameterless_Returns_TaskOfInt32()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_TaskOfInt32))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_TaskOfInt32),
            static (instance, context, _) => instance.Method_Parameterless_Returns_TaskOfInt32());
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(Nil.Default), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_TaskOfInt32) + " called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, {Int32:12345}]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            MessagePackSerializer.Serialize(ref writer, 12345);
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }
    [Fact]
    public async Task Parameterless_Returns_ValueTask()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTask))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTask),
            static (instance, context, _) => instance.Method_Parameterless_Returns_ValueTask());
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(Nil.Default), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTask) + " called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameterless_Returns_ValueTaskOfInt32()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTaskOfInt32))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTaskOfInt32),
            static (instance, context, _) => instance.Method_Parameterless_Returns_ValueTaskOfInt32());
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(Nil.Default), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Returns_ValueTaskOfInt32) + " called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, {Int32:12345}]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            MessagePackSerializer.Serialize(ref writer, 12345);
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Single_Returns_Task()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Returns_Task))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Returns_Task),
            static (instance, context, request) => instance.Method_Parameter_Single_Returns_Task(request));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(12345), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Returns_Task) + "(12345) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Multiple_Returns_Task()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_Task))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_Task),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Returns_Task(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string, bool>(12345, "テスト", true)), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_Task) + "(12345,テスト,True) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();
            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Multiple_Returns_TaskOfInt32()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Returns_TaskOfInt32(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string, bool>(12345, "テスト", true)), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32) + "(12345,テスト,True) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, {Int32:12345}]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            MessagePackSerializer.Serialize(ref writer, 12345);
            writer.Flush();
            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task CallRepeated_Parameter_Multiple_Returns_TaskOfInt32()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Returns_TaskOfInt32(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        for (var i = 0; i < 3; i++)
        {
            var ctx = new StreamingHubContext();
            ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string, bool>(i, $"テスト{i}", i % 2 == 0)), DateTime.Now, i * 1000);
            await handler.MethodBody.Invoke(ctx);
        }

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32) + "(0,テスト0,True) called.");
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32) + "(1,テスト1,False) called.");
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32) + "(2,テスト2,True) called.");

        byte[] BuildMessage(int messageId, int retVal)
        {
            // [MessageId, MethodId, {Int32:RetVal}]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(messageId);
            writer.Write(FNV1A32.GetHashCode(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32)) /* MethodId */);
            MessagePackSerializer.Serialize(ref writer, retVal);
            writer.Flush();
            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage(0, 0));
        fakeStreamingHubContext.Responses[1].Memory.ToArray().Should().Equal(BuildMessage(1000, 1));
        fakeStreamingHubContext.Responses[2].Memory.ToArray().Should().Equal(BuildMessage(2000, 2));
    }

    [Fact]
    public async Task Parameterless_Void()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Void))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameterless_Void),
            static (instance, context, _) => instance.Method_Parameterless_Void());
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(Nil.Default), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameterless_Void) + " called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Single_Void()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Void))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Void),
            static (instance, context, request) => instance.Method_Parameter_Single_Void(request));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(12345), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Single_Void) + "(12345) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Multiple_Void()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Void(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string, bool>(12345, "テスト", true)), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void) + "(12345,テスト,True) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, Nil]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.WriteNil();
            writer.Flush();

            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public async Task Parameter_Multiple_Void_Without_MessageId()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Void(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, MessagePackMagicOnionSerializerProvider.Default.Create(MethodType.DuplexStreaming, null), serviceProvider);

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, MessagePackSerializer.Serialize(new DynamicArgumentTuple<int, string, bool>(12345, "テスト", true)), DateTime.Now, -1 /* The client requires no response */);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Void) + "(12345,テスト,True) called.");
        fakeStreamingHubContext.Responses.Should().BeEmpty();
    }

    [Fact]
    public async Task UseCustomMessageSerializer()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, DynamicArgumentTuple<int, string, bool>, int>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32),
            static (instance, context, request) => instance.Method_Parameter_Multiple_Returns_TaskOfInt32(request.Item1, request.Item2, request.Item3));
        var hubInstance = new StreamingHubHandlerTestHub();
        var fakeStreamingHubContext = new FakeStreamingServiceContext<StreamingHubPayload, StreamingHubPayload>(hubType, hubMethodInfo, XorMessagePackMagicOnionSerializerProvider.Instance.Create(MethodType.DuplexStreaming, null), serviceProvider);
        var bufferWriter = new ArrayBufferWriter<byte>();
        var serializer = XorMessagePackMagicOnionSerializerProvider.Instance.Create(MethodType.DuplexStreaming, null);
        serializer.Serialize(bufferWriter, new DynamicArgumentTuple<int, string, bool>(12345, "テスト", true));

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()
        {
            MessageSerializer = XorMessagePackMagicOnionSerializerProvider.Instance,
        }), serviceProvider);
        var ctx = new StreamingHubContext();
        ctx.Initialize(handler, fakeStreamingHubContext, hubInstance, bufferWriter.WrittenMemory.ToArray(), DateTime.Now, 0);
        await handler.MethodBody.Invoke(ctx);

        // Assert
        hubInstance.Results.Should().Contain(nameof(StreamingHubHandlerTestHub.Method_Parameter_Multiple_Returns_TaskOfInt32) + "(12345,テスト,True) called.");
        byte[] BuildMessage()
        {
            // [MessageId, MethodId, {Xor:Int32:12345}]
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);
            writer.WriteArrayHeader(3);
            writer.Write(ctx.MessageId);
            writer.Write(ctx.MethodId);
            writer.Flush();
            serializer.Serialize(buffer, 12345);
            return buffer.WrittenMemory.ToArray();
        }
        fakeStreamingHubContext.Responses[0].Memory.ToArray().Should().Equal(BuildMessage());
    }

    [Fact]
    public void MethodAttributeLookup()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var hubType = typeof(StreamingHubHandlerTestHub);
        var hubMethodInfo = hubType.GetMethod(nameof(StreamingHubHandlerTestHub.Method_Attribute))!;
        var hubMethod = new MagicOnionStreamingHubMethod<StreamingHubHandlerTestHub, MessagePack.Nil>(
            nameof(StreamingHubHandlerTestHub), nameof(StreamingHubHandlerTestHub.Method_Attribute),
            static (instance, context, _) => instance.Method_Attribute());

        // Act
        var handler = new StreamingHubHandler(hubMethod, new StreamingHubHandlerOptions(new MagicOnionOptions()), serviceProvider);

        // Assert
        Assert.NotEmpty(handler.AttributeLookup);
        Assert.NotEmpty(handler.AttributeLookup[typeof(CustomMethodAttribute)]);
        Assert.NotEmpty(handler.AttributeLookup[typeof(CustomHubAttribute)]);
    }

    interface IStreamingHubHandlerTestHubReceiver
    {
    }

    interface IStreamingHubHandlerTestHub : IStreamingHub<IStreamingHubHandlerTestHub, IStreamingHubHandlerTestHubReceiver>
    {
        Task Method_Parameterless_Returns_Task();
        Task<int> Method_Parameterless_Returns_TaskOfInt32();

        Task Method_Parameter_Single_Returns_Task(int arg0);
        Task Method_Parameter_Multiple_Returns_Task(int arg0, string arg1, bool arg2);
        Task<int> Method_Parameter_Multiple_Returns_TaskOfInt32(int arg0, string arg1, bool arg2);

        ValueTask Method_Parameterless_Returns_ValueTask();
        ValueTask<int> Method_Parameterless_Returns_ValueTaskOfInt32();

        void Method_Parameterless_Void();
        void Method_Parameter_Single_Void(int arg0);
        void Method_Parameter_Multiple_Void(int arg0, string arg1, bool arg2);

        Task Method_Attribute();
    }

    [AttributeUsage(AttributeTargets.Class)]
    class CustomHubAttribute : Attribute;
    [AttributeUsage(AttributeTargets.Method)]
    class CustomMethodAttribute : Attribute;

    [CustomHub]
    class StreamingHubHandlerTestHub : IStreamingHubHandlerTestHub
    {
        public List<string> Results { get; } = new List<string>();

        public IStreamingHubHandlerTestHub FireAndForget() => throw new NotImplementedException();
        public Task DisposeAsync() => throw new NotImplementedException();
        public Task WaitForDisconnect() => throw new NotImplementedException();

        public Task Method_Parameterless_Returns_Task()
        {
            Results.Add(nameof(Method_Parameterless_Returns_Task) + " called.");
            return Task.CompletedTask;
        }

        public Task<int> Method_Parameterless_Returns_TaskOfInt32()
        {
            Results.Add(nameof(Method_Parameterless_Returns_TaskOfInt32) + " called.");
            return Task.FromResult(12345);
        }

        public Task Method_Parameter_Single_Returns_Task(int arg0)
        {
            Results.Add(nameof(Method_Parameter_Single_Returns_Task) + $"({arg0}) called.");
            return Task.CompletedTask;
        }

        public Task Method_Parameter_Multiple_Returns_Task(int arg0, string arg1, bool arg2)
        {
            Results.Add(nameof(Method_Parameter_Multiple_Returns_Task) + $"({arg0},{arg1},{arg2}) called.");
            return Task.CompletedTask;
        }

        public Task<int> Method_Parameter_Multiple_Returns_TaskOfInt32(int arg0, string arg1, bool arg2)
        {
            Results.Add(nameof(Method_Parameter_Multiple_Returns_TaskOfInt32) + $"({arg0},{arg1},{arg2}) called.");
            return Task.FromResult(arg0);
        }

        public ValueTask Method_Parameterless_Returns_ValueTask()
        {
            Results.Add(nameof(Method_Parameterless_Returns_ValueTask) + " called.");
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> Method_Parameterless_Returns_ValueTaskOfInt32()
        {
            Results.Add(nameof(Method_Parameterless_Returns_ValueTaskOfInt32) + " called.");
            return ValueTask.FromResult(12345);
        }

        public void Method_Parameterless_Void()
        {
            Results.Add(nameof(Method_Parameterless_Void) + " called.");
        }

        public void Method_Parameter_Single_Void(int arg0)
        {
            Results.Add(nameof(Method_Parameter_Single_Void) + $"({arg0}) called.");
        }

        public void Method_Parameter_Multiple_Void(int arg0, string arg1, bool arg2)
        {
            Results.Add(nameof(Method_Parameter_Multiple_Void) + $"({arg0},{arg1},{arg2}) called.");
        }

        [CustomMethod]
        public Task Method_Attribute() => Task.CompletedTask;
    }
}
