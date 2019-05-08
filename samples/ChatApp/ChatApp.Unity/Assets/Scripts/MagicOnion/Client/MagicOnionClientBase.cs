using Grpc.Core;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Threading;

namespace MagicOnion.Client
{
    public abstract class MagicOnionClientBase<T> where T : IService<T>
    {
        protected string host;
        protected CallOptions option;
        protected CallInvoker callInvoker;
        protected IFormatterResolver resolver;

        protected MagicOnionClientBase()
        {

        }

        protected MagicOnionClientBase(CallInvoker callInvoker, IFormatterResolver resolver)
        {
            this.callInvoker = callInvoker;
            this.resolver = resolver;
        }

        protected abstract MagicOnionClientBase<T> Clone();

        public virtual T WithCancellationToken(CancellationToken cancellationToken)
        {
            return WithOptions(this.option.WithCancellationToken(cancellationToken));
        }

        public virtual T WithDeadline(DateTime deadline)
        {
            return WithOptions(this.option.WithDeadline(deadline));
        }

        public virtual T WithHeaders(Metadata headers)
        {
            return WithOptions(this.option.WithHeaders(headers));
        }

        public virtual T WithHost(string host)
        {
            var newInstance = Clone();
            newInstance.host = host;
            return (T)(object)newInstance;
        }

        public virtual T WithOptions(CallOptions option)
        {
            var newInstance = Clone();
            newInstance.option = option;
            return (T)(object)newInstance;
        }
    }
}