using Grpc.Core;
using MagicOnion.Client.EmbeddedServices;
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
        DuplexStreamingResult<bool, bool> latestStreamingResult;
        AsyncSubject<Unit> waitConnectComplete;
        IDisposable connectingTask;
        GrpcCancellationTokenSource cancellationTokenSource = new GrpcCancellationTokenSource();
        LinkedList<Action> disconnectedActions = new LinkedList<Action>();

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

        public ChannelContext(Channel channel, Func<string> connectionIdFactory = null, bool useSameId = true)
        {
            this.channel = channel;
            this.useSameId = useSameId;
            this.connectionIdFactory = connectionIdFactory ?? (() => Guid.NewGuid().ToString());
            if (useSameId)
            {
                this.connectionId = this.connectionIdFactory();
            }
            this.waitConnectComplete = new AsyncSubject<Unit>();
            connectingTask = Observable.FromCoroutine(() => ConnectAlways()).Subscribe(_ => { }, ex =>
            {
                UnityEngine.Debug.Log("Error Detected:" + ex.ToString());
            }, () => { });

            // destructor
            this.channel.ShutdownToken.Register(() =>
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

        IEnumerator ConnectAlways()
        {
            while (true)
            {
                if (isDisposed) yield break;
                if (channel.State == ChannelState.Shutdown) yield break;

                var conn = channel.ConnectAsync().ToYieldInstruction(false);
                yield return conn;

                if (isDisposed) yield break;
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

                var connectCheck = heartBeatConnect.Result.ResponseStream.MoveNext().ToYieldInstruction(false);
                yield return connectCheck;
                if (connectCheck.HasError)
                {
                    GrpcEnvironment.Logger.Error(heartBeatConnect.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                    continue;
                }

                this.connectionId = connectionId;
                currentRetryCount = 0;

                waitConnectComplete.OnNext(Unit.Default);
                waitConnectComplete.OnCompleted();

                var waitForDisconnect = Observable.Amb(channel.WaitForStateChangedAsync(ChannelState.Ready), heartBeatConnect.Result.ResponseStream.MoveNext()).ToYieldInstruction(false);
                yield return waitForDisconnect;
                try
                {
                    waitConnectComplete = new AsyncSubject<Unit>();
                    foreach (var action in disconnectedActions)
                    {
                        action();
                    }
                }
                catch (Exception ex)
                {
                    GrpcEnvironment.Logger.Error(ex, "Reconnect Failed, Retrying:" + currentRetryCount++);
                }

                if (waitForDisconnect.HasError)
                {
                    GrpcEnvironment.Logger.Error(waitForDisconnect.Error, "Reconnect Failed, Retrying:" + currentRetryCount++);
                }
            }
        }

        // TODO:more createClient overload.
        public T CreateClient<T>()
            where T : IService<T>
        {
            return MagicOnionClient.Create<T>(channel)
                .WithHeaders(new Metadata { { ChannelContext.HeaderKey, ConnectionId } });
        }

        public T CreateClient<T>(Metadata metadata)
            where T : IService<T>
        {
            var newMetadata = new Metadata();
            for (int i = 0; i < metadata.Count; i++)
            {
                newMetadata.Add(metadata[i]);
            }
            newMetadata.Add(ChannelContext.HeaderKey, ConnectionId);

            return MagicOnionClient.Create<T>(channel).WithHeaders(newMetadata).WithCancellationToken(cancellationTokenSource.Token);
        }


        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            connectingTask.Dispose();
            cancellationTokenSource.Cancel();
            waitConnectComplete.Dispose();
            latestStreamingResult.Dispose();
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
                    node.List.Remove(node);
                    node = null;
                }
            }
        }
    }
}