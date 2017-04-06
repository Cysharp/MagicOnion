using Grpc.Core;
using MagicOnion.Client.EmbeddedServices;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace MagicOnion.Client
{
    public class ChannelContext : IDisposable
    {
        public const string HeaderKey = "connection_id";

        readonly Channel channel;
        readonly bool useSameId;
        readonly Func<string> connectionIdFactory;

        bool isDisposed;
        int currentRetryCount = 0;
        int pingSecond;
        DuplexStreamingResult<Nil, Nil> latestStreamingResult;
        AsyncSubject<Unit> waitConnectComplete;
        IDisposable connectingTask;
        GrpcCancellationTokenSource cancellationTokenSource = new GrpcCancellationTokenSource();
        LinkedList<Action> disconnectedActions = new LinkedList<Action>();

        public bool IsDisposed { get { return isDisposed; } }

        string connectionId;
        public string ConnectionId
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException("ChannelContext");
                return connectionId;
            }
        }

        public Channel Channel
        {
            get
            {
                return channel;
            }
        }

        public ChannelContext(Channel channel, Func<string> connectionIdFactory = null, bool useSameId = true, int pingSecond = 15)
        {
            this.channel = channel;
            this.useSameId = useSameId;
            this.pingSecond = pingSecond;
            this.connectionIdFactory = connectionIdFactory ?? (() => Guid.NewGuid().ToString());
            if (useSameId)
            {
                this.connectionId = this.connectionIdFactory();
            }
            this.waitConnectComplete = new AsyncSubject<Unit>();
            connectingTask = Observable.FromCoroutine(token => ConnectAlways(token), false).Subscribe(_ => { }, ex =>
            {
                UnityEngine.Debug.Log("Error Detected:" + ex.ToString());
            }, () => { });

            // destructor
            this.channel.ShutdownToken.RegisterLast(() =>
            {
                this.Dispose();
            });
        }

        public IObservable<Unit> WaitConnectComplete()
        {
            return waitConnectComplete;
        }

        public IDisposable RegisterDisconnectedAction(Action action)
        {
            var node = disconnectedActions.AddLast(action);
            return new UnregisterToken(node);
        }

        IEnumerator ConnectAlways(CancellationToken token)
        {
            while (true)
            {
                if (isDisposed) yield break;
                if (channel.State == ChannelState.Shutdown) yield break;
                if (token.IsCancellationRequested) yield break;

                var conn = channel.ConnectAsync().ToYieldInstruction(false);
                yield return conn;

                if (isDisposed) yield break;
                if (token.IsCancellationRequested) yield break;
                if (conn.HasError)
                {
                    GrpcEnvironment.Logger.Error(conn.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                    continue;
                }

                var connectionId = (useSameId) ? this.connectionId : connectionIdFactory();
                var client = new HeartbeatClient(channel, connectionId);
                latestStreamingResult.Dispose();
                var heartBeatConnect = client.Connect().ToYieldInstruction(false);
                yield return heartBeatConnect;
                if (heartBeatConnect.HasError)
                {
                    GrpcEnvironment.Logger.Error(heartBeatConnect.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                    continue;
                }
                else
                {
                    latestStreamingResult = heartBeatConnect.Result;
                }
                if (token.IsCancellationRequested) yield break;

                var connectCheck = heartBeatConnect.Result.ResponseStream.MoveNext().ToYieldInstruction(false);
                yield return connectCheck;
                if (connectCheck.HasError)
                {
                    GrpcEnvironment.Logger.Error(heartBeatConnect.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                    continue;
                }
                if (token.IsCancellationRequested) yield break;

                this.connectionId = connectionId;
                currentRetryCount = 0;

                waitConnectComplete.OnNext(Unit.Default);
                waitConnectComplete.OnCompleted();

                var heartbeat = Observable.FromCoroutine((ct) => PingLoop(latestStreamingResult, ct)).Select(_ => true);
                var waitForDisconnect = Observable.Amb(channel.WaitForStateChangedAsync(ChannelState.Ready), heartBeatConnect.Result.ResponseStream.MoveNext(), heartbeat).ToYieldInstruction(false);
                yield return waitForDisconnect;
                try
                {
                    waitConnectComplete = new AsyncSubject<Unit>();
                    foreach (var action in disconnectedActions)
                    {
                        action();
                    }
                    disconnectedActions.Clear();
                }
                catch (Exception ex)
                {
                    GrpcEnvironment.Logger.Error(ex, "Reconnect Failed, Retrying:" + currentRetryCount++);
                }

                if (waitForDisconnect.HasError)
                {
                    GrpcEnvironment.Logger.Error(waitForDisconnect.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                }

                if (token.IsCancellationRequested) yield break;
            }
        }

        IEnumerator PingLoop(DuplexStreamingResult<Nil, Nil> streaming, CancellationToken token)
        {
            var waiter = new UnityEngine.WaitForSeconds(pingSecond);
            while (true)
            {
                if (token.IsCancellationRequested) yield break;
                yield return waiter;

                if (token.IsCancellationRequested) yield break;

                var r = streaming.RequestStream.WriteAsync(Nil.Default).ToYieldInstruction(false);
                yield return r;
                if (r.HasError) yield break;
            }
        }

        public T CreateClient<T>()
            where T : IService<T>
        {
            return CreateClient<T>(MessagePackSerializer.DefaultResolver);
        }

        public T CreateClient<T>(Func<Channel, CallInvoker> callInvokerFactory)
            where T : IService<T>
        {
            return CreateClient<T>(callInvokerFactory, MessagePackSerializer.DefaultResolver);
        }

        public T CreateClient<T>(IFormatterResolver resolver)
            where T : IService<T>
        {
            return MagicOnionClient.Create<T>(channel, resolver)
                .WithHeaders(new Metadata { { ChannelContext.HeaderKey, ConnectionId } })
                .WithCancellationToken(cancellationTokenSource.Token);
        }

        public T CreateClient<T>(Func<Channel, CallInvoker> callInvokerFactory, IFormatterResolver resolver)
            where T : IService<T>
        {
            return MagicOnionClient.Create<T>(callInvokerFactory(channel), resolver)
                .WithHeaders(new Metadata { { ChannelContext.HeaderKey, ConnectionId } })
                .WithCancellationToken(cancellationTokenSource.Token);
        }

        public T CreateClient<T>(Metadata metadata)
            where T : IService<T>
        {
            return CreateClient<T>(metadata, MessagePackSerializer.DefaultResolver);
        }

        public T CreateClient<T>(Func<Channel, CallInvoker> callInvokerFactory, Metadata metadata)
            where T : IService<T>
        {
            return CreateClient<T>(callInvokerFactory, metadata, MessagePackSerializer.DefaultResolver);
        }

        public T CreateClient<T>(Metadata metadata, IFormatterResolver resolver)
            where T : IService<T>
        {
            var newMetadata = new Metadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                newMetadata.Add(metadata[i]);
            }
            newMetadata.Add(ChannelContext.HeaderKey, ConnectionId);

            return MagicOnionClient.Create<T>(channel, resolver).WithHeaders(newMetadata).WithCancellationToken(cancellationTokenSource.Token);
        }

        public T CreateClient<T>(Func<Channel, CallInvoker> callInvokerFactory, Metadata metadata, IFormatterResolver resolver)
            where T : IService<T>
        {
            var newMetadata = new Metadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                newMetadata.Add(metadata[i]);
            }
            newMetadata.Add(ChannelContext.HeaderKey, ConnectionId);

            return MagicOnionClient.Create<T>(callInvokerFactory(channel), resolver).WithHeaders(newMetadata).WithCancellationToken(cancellationTokenSource.Token);
        }


        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            connectingTask.Dispose();
            cancellationTokenSource.Cancel();
            waitConnectComplete.Dispose();
            latestStreamingResult.Dispose();

            foreach (var action in disconnectedActions)
            {
                action();
            }
            disconnectedActions.Clear();
        }

        class UnregisterToken : IDisposable
        {
            LinkedListNode<Action> node;

            public UnregisterToken(LinkedListNode<Action> node)
            {
                this.node = node;
            }

            public void Dispose()
            {
                if (node != null)
                {
                    if (node.List != null)
                    {
                        node.List.Remove(node);
                    }
                    node = null;
                }
            }
        }
    }
}