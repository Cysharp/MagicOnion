using System;

namespace MagicOnion.Client
{
    /// <summary>
    /// The interface of the handler for StreamingHub diagnostics.
    /// </summary>
    public interface IStreamingHubDiagnosticHandler
    {
        /// <summary>
        /// The callback method at the beginning of a Hub method request.
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="hubInstance"></param>
        /// <param name="requestId"></param>
        /// <param name="methodName"></param>
        /// <param name="request"></param>
        /// <param name="isFireAndForget"></param>
        void OnRequestBegin<THub, TRequest>(THub hubInstance, Guid requestId, string methodName, TRequest request, bool isFireAndForget);

        /// <summary>
        /// The callback method at the end of a Hub method request.
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="hubInstance"></param>
        /// <param name="requestId"></param>
        /// <param name="methodName"></param>
        /// <param name="response"></param>
        void OnRequestEnd<THub, TResponse>(THub hubInstance, Guid requestId, string methodName, TResponse response);

        /// <summary>
        /// The callback method when a method of HubReceiver is invoked.
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="hubInstance"></param>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        void OnBroadcastEvent<THub, T>(THub hubInstance, string methodName, T value);
    }
}
