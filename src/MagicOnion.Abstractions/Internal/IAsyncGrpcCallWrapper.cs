using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Internal
{
    public interface IAsyncGrpcCallWrapper : IDisposable
    {
        Task<Metadata> ResponseHeadersAsync { get; }
        Status GetStatus();
        Metadata GetTrailers();
    }

    public interface IAsyncClientStreamingCallWrapper<TRequest, TResponse> : IAsyncGrpcCallWrapper
    {
        IClientStreamWriter<TRequest> RequestStream { get; }
        Task<TResponse> ResponseAsync { get; }
    }

    public interface IAsyncServerStreamingCallWrapper<TResponse> : IAsyncGrpcCallWrapper
    {
        IAsyncStreamReader<TResponse> ResponseStream { get; }
    }

    public interface IAsyncDuplexStreamingCallWrapper<TRequest, TResponse> : IAsyncGrpcCallWrapper
    {
        IClientStreamWriter<TRequest> RequestStream { get; }
        IAsyncStreamReader<TResponse> ResponseStream { get; }
    }
}
