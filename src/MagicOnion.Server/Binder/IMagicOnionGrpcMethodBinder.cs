namespace MagicOnion.Server.Binder;

public interface IMagicOnionGrpcMethodBinder<TService>
    where TService : class
{
    void BindUnary<TRequest, TResponse, TRawRequest, TRawResponse>(IMagicOnionUnaryMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class;
    void BindClientStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionClientStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class;
    void BindServerStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionServerStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class;
    void BindDuplexStreaming<TRequest, TResponse, TRawRequest, TRawResponse>(MagicOnionDuplexStreamingMethod<TService, TRequest, TResponse, TRawRequest, TRawResponse> method)
        where TRawRequest : class
        where TRawResponse : class;
    void BindStreamingHub(MagicOnionStreamingHubConnectMethod<TService> method);
}
