using Benchmark.ClientLib.Reports;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Internal.Runtime
{
    public class AsyncUnaryCallWorkerPool<TRequest, TReply> : IDisposable
    {
        private readonly int _workerCount;
        private readonly CancellationToken _ct;
        private readonly TaskCompletionSource _timeoutTcs = new TaskCompletionSource();
        private readonly TaskCompletionSource _completeTask = new TaskCompletionSource();
        private int _completeCount;

        private readonly Channel<Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>>> _channel;
        private readonly ChannelWriter<Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>>> _writer;
        private readonly ChannelReader<Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>>> _reader;

        private readonly ConcurrentBag<CallResult> _results = new ConcurrentBag<CallResult>();

        public Func<(int current, int completed), bool> CompleteCondition { get; init; } = (x) => false;
        public int CompleteCount => _completeCount;
        public bool Timeouted => _timeoutTcs.Task.IsCompleted;
        public bool Completed => _completeTask.Task.IsCompleted;

        public AsyncUnaryCallWorkerPool(int workerCount, CancellationToken ct) : this(workerCount, 1000, ct)
        {
        }

        public AsyncUnaryCallWorkerPool(int workerCount, int channelSize, CancellationToken ct)
        {
            _workerCount = workerCount;
            _ct = ct;
            _ct.Register(() => _timeoutTcs.TrySetResult());
            _channel = System.Threading.Channels.Channel.CreateBounded<Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>>>(new BoundedChannelOptions(channelSize)
            {
                SingleReader = false,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            });
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        /// <summary>
        /// Wait for Pool complete
        /// </summary>
        /// <returns></returns>
        public Task WaitForCompleteAsync() => _completeTask.Task;

        /// <summary>
        /// Wait for Pool end by timeout
        /// </summary>
        /// <returns></returns>
        public Task WaitForTimeout() => _timeoutTcs.Task;

        public void RunWorkers(Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>> action, TRequest request, CancellationToken ct)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            RunCore(action, request, ct);
            WatchComplete();
        }

        public CallResult[] GetResult()
        {
            return _results.ToArray();
        }

        public void Dispose()
        {
            _writer.TryComplete();
            _results.Clear();
        }

        /// <summary>
        /// Main execution
        /// </summary>
        /// <returns></returns>
        private void RunCore(Func<int, TRequest, CancellationToken, AsyncUnaryCall<TReply>> action, TRequest request, CancellationToken ct)
        {
            // write
            Task.Run(async () =>
            {
                while (await _writer.WaitToWriteAsync(_ct).ConfigureAwait(false))
                {
                    try
                    {
                        if (_ct.IsCancellationRequested)
                            return;

                        await _writer.WriteAsync(action, _ct).ConfigureAwait(false);
                    }
                    catch (ChannelClosedException)
                    {
                        // already closed.
                    }
                    catch (OperationCanceledException)
                    {
                        // canceled
                    }
                }
            }, _ct);

            // read
            var workerId = 0;
            for (var i = 0; i < _workerCount; i++)
            {
                var id = workerId++;
                Task.Run(async () =>
                {
                    while (await _reader.WaitToReadAsync(_ct))
                    {
                        var sw = ValueStopwatch.StartNew();
                        Exception error = null;
                        Status status = Status.DefaultSuccess;
                        try
                        {
                            var item = await _reader.ReadAsync(_ct);

                            if (_ct.IsCancellationRequested)
                                return;

                            await item.Invoke(id, request, ct);
                            //Console.WriteLine($"done {_completeCount} ({_reader.Count}, id {id})");
                        }
                        catch (RpcException rex)
                        {
                            error = rex;
                            status = rex.Status;
                        }
                        catch (OperationCanceledException oex)
                        {
                            error = oex;
                            status = Status.DefaultCancelled;
                        }
                        catch (ChannelClosedException)
                        {
                            // already closed.
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                            status = new Status(StatusCode.Unknown, ex.Message);
                        }
                        finally
                        {
                            Interlocked.Increment(ref _completeCount);
                            _results.Add(new CallResult
                            {
                                Duration = sw.Elapsed,
                                Error = error,
                                Status = status,
                                TimeStamp = DateTime.UtcNow,
                            });
                        }
                    }
                }, _ct);
            }
        }

        private void WatchComplete()
        {
            // complete
            Task.Run(async () =>
            {
                while (!CompleteCondition((_reader.Count, _completeCount)))
                {
                    if (_ct.IsCancellationRequested)
                    {
                        _completeTask.SetCanceled();
                        return;
                    }

                    await Task.Delay(100);
                }
                _writer.TryComplete();
                _completeTask.SetResult();
            }, _ct);
        }
    }
}
