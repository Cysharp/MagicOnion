using Grpc.Core;
using MagicOnion.Server;
using System;
using System.Threading;

namespace MagicOnion.Client
{
    public abstract class MagicOnionClientBase<T> where T : IService<T>
    {
        protected string host;
        protected CallOptions option;
        protected CallInvoker callInvoker;

        protected MagicOnionClientBase()
        {

        }

        protected MagicOnionClientBase(CallInvoker callInvoker)
        {
            this.callInvoker = callInvoker;
        }

        protected abstract MagicOnionClientBase<T> Clone();

        public T WithCancellationToken(CancellationToken cancellationToken)
        {
            return WithOption(this.option.WithCancellationToken(cancellationToken));
        }

        public T WithDeadline(DateTime deadline)
        {
            return WithOption(this.option.WithDeadline(deadline));
        }

        public T WithHeaders(Metadata headers)
        {
            return WithOption(this.option.WithHeaders(headers));
        }

        public T WithHost(string host)
        {
            var newInstance = Clone();
            newInstance.host = host;
            return (T)(object)newInstance;
        }

        public T WithOption(CallOptions option)
        {
            var newInstance = Clone();
            newInstance.option = option;
            return (T)(object)newInstance;
        }
    }
}