using Grpc.Core;
using MessagePack;
using System;
using System.IO;
using System.Threading.Tasks;
using MagicOnion.Utils;
using Microsoft.AspNetCore.Connections;

namespace MagicOnion.Server.Hubs
{
    public abstract class StreamingHubBase<THubInterface, TReceiver> : ServiceBase<THubInterface>, IStreamingHub<THubInterface, TReceiver>
        where THubInterface : IStreamingHub<THubInterface, TReceiver>
    {
        static protected readonly Task<Nil> NilTask = Task.FromResult(Nil.Default);
        static protected readonly ValueTask CompletedTask = new ValueTask();

        static readonly Metadata ResponseHeaders = new Metadata()
        {
            { "x-magiconion-streaminghub-version", "2" },
        };

        public HubGroupRepository Group { get; private set; } = default!; /* lateinit */

        protected Guid ConnectionId { get { return Context.ContextId; } }

        // Broadcast Commands

        [Ignore]
        protected TReceiver Broadcast(IGroup group)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType;
            return (TReceiver)Activator.CreateInstance(type, group)!;
        }

        [Ignore]
        protected TReceiver BroadcastExceptSelf(IGroup group)
        {
            return BroadcastExcept(group, Context.ContextId);
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid except)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptOne;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, except })!;
        }

        [Ignore]
        protected TReceiver BroadcastExcept(IGroup group, Guid[] excepts)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ExceptMany;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, excepts })!;
        }

        [Ignore]
        protected TReceiver BroadcastToSelf(IGroup group)
        {
            return BroadcastTo(group, Context.ContextId);
        }

        [Ignore]
        protected TReceiver BroadcastTo(IGroup group, Guid toConnectionId)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToOne;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, toConnectionId })!;
        }

        [Ignore]
        protected TReceiver BroadcastTo(IGroup group, Guid[] toConnectionIds)
        {
            var type = DynamicBroadcasterBuilder<TReceiver>.BroadcasterType_ToMany;
            return (TReceiver)Activator.CreateInstance(type, new object[] { group, toConnectionIds })!;
        }

        /// <summary>
        /// Called before connect, instead of constructor.
        /// </summary>
        protected virtual ValueTask OnConnecting()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Called after disconnect.
        /// </summary>
        protected virtual ValueTask OnDisconnected()
        {
            return CompletedTask;
        }

        public async Task<DuplexStreamingResult<byte[], byte[]>> Connect()
        {
            var streamingContext = GetDuplexStreamingContext<byte[], byte[]>();

            var group = StreamingHubHandlerRepository.GetGroupRepository(Context.MethodHandler);
            this.Group = new HubGroupRepository(this.Context, group);
            try
            {
                await OnConnecting();
                await HandleMessageAsync();
            }
            catch (OperationCanceledException)
            {
                // NOTE: If DuplexStreaming is disconnected by the client, OperationCanceledException will be thrown.
                //       However, such behavior is expected. the exception can be ignored.
            }
            catch (IOException ex) when (ex.InnerException is ConnectionAbortedException)
            {
                // NOTE: If DuplexStreaming is disconnected by the client, IOException will be thrown.
                //       However, such behavior is expected. the exception can be ignored.
            }
            finally
            {
                Context.CompleteStreamingHub();
                await OnDisconnected();
                await this.Group.DisposeAsync();
            }

            return streamingContext.Result();
        }

        async Task HandleMessageAsync()
        {
            var ct = Context.CallContext.CancellationToken;
            var reader = Context.RequestStream!;
            var writer = Context.ResponseStream!;

            // Send a hint to the client to start sending messages.
            // The client can read the response headers before any StreamingHub's message.
            await Context.CallContext.WriteResponseHeadersAsync(ResponseHeaders);

            // Write a marker that is the beginning of the stream.
            // NOTE: To prevent buffering by AWS ALB or reverse-proxy.
            static byte[] BuildMarkerResponse()
            {
                using (var buffer = ArrayPoolBufferWriter.RentThreadStaticWriter())
                {
                    var writer = new MessagePackWriter(buffer);

                    // response:  [messageId, methodId, response]
                    // HACK: If the ID of the message is `-1`, the client will ignore the message.
                    writer.WriteArrayHeader(3);
                    writer.Write(-1);
                    writer.Write(0);
                    writer.WriteNil();
                    writer.Flush();
                    return buffer.WrittenSpan.ToArray();
                }
            }
            await writer.WriteAsync(BuildMarkerResponse());

            var handlers = StreamingHubHandlerRepository.GetHandlers(Context.MethodHandler);

            // Main loop of StreamingHub.
            // Be careful to allocation and performance.
            while (await reader.MoveNext(ct)) // must keep SyncContext.
            {
                (int methodId, int messageId, int offset) FetchHeader(byte[] msgData)
                {
                    var messagePackReader = new MessagePackReader(msgData);

                    var length = messagePackReader.ReadArrayHeader();
                    if (length == 2)
                    {
                        // void: [methodId, [argument]]
                        var mid = messagePackReader.ReadInt32();
                        var consumed = (int)messagePackReader.Consumed;

                        return (mid, -1, consumed);
                    }
                    else if (length == 3)
                    {
                        // T: [messageId, methodId, [argument]]
                        var msgId = messagePackReader.ReadInt32();
                        var metId = messagePackReader.ReadInt32();
                        var consumed = (int)messagePackReader.Consumed;
                        return (metId, msgId, consumed);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid data format.");
                    }
                }

                var data = reader.Current;
                var (methodId, messageId, offset) = FetchHeader(data);

                if (messageId == -1)
                {
                    if (handlers.TryGetValue(methodId, out var handler))
                    {
                        var context = new StreamingHubContext() // create per invoke.
                        {
                            SerializerOptions = handler.serializerOptions,
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            Path = handler.ToString(),
                            MethodId = handler.MethodId,
                            MessageId = -1,
                            Timestamp = DateTime.UtcNow
                        };

                        var isErrorOrInterrupted = false;
                        Context.MethodHandler.logger.BeginInvokeHubMethod(context, context.Request, handler.RequestType);
                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            isErrorOrInterrupted = true;
                            Context.MethodHandler.logger.Error(ex, context);
                        }
                        finally
                        {
                            Context.MethodHandler.logger.EndInvokeHubMethod(context, context.responseSize, context.responseType, (DateTime.UtcNow - context.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
                    }
                }
                else
                {
                    if (handlers.TryGetValue(methodId, out var handler))
                    {
                        var context = new StreamingHubContext() // create per invoke.
                        {
                            SerializerOptions = handler.serializerOptions,
                            HubInstance = this,
                            ServiceContext = Context,
                            Request = new ArraySegment<byte>(data, offset, data.Length - offset),
                            Path = handler.ToString(),
                            MethodId = handler.MethodId,
                            MessageId = messageId,
                            Timestamp = DateTime.UtcNow
                        };

                        var isErrorOrInterrupted = false;
                        Context.MethodHandler.logger.BeginInvokeHubMethod(context, context.Request, handler.RequestType);
                        try
                        {
                            await handler.MethodBody.Invoke(context);
                        }
                        catch (ReturnStatusException ex)
                        {
                            await context.WriteErrorMessage((int)ex.StatusCode, ex.Detail, null, false);
                        }
                        catch (Exception ex)
                        {
                            isErrorOrInterrupted = true;
                            Context.MethodHandler.logger.Error(ex, context);
                            await context.WriteErrorMessage((int)StatusCode.Internal, $"An error occurred while processing handler '{handler.ToString()}'.", ex, Context.MethodHandler.isReturnExceptionStackTraceInErrorDetail);
                        }
                        finally
                        {
                            Context.MethodHandler.logger.EndInvokeHubMethod(context, context.responseSize, context.responseType, (DateTime.UtcNow - context.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Handler not found in received methodId, methodId:" + methodId);
                    }
                }
            }
        }

        // Interface methods for Client

        THubInterface IStreamingHub<THubInterface, TReceiver>.FireAndForget()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        Task IStreamingHub<THubInterface, TReceiver>.DisposeAsync()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }

        Task IStreamingHub<THubInterface, TReceiver>.WaitForDisconnect()
        {
            throw new NotSupportedException("Invoke from client proxy only");
        }
    }
}
