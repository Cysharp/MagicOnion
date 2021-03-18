using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace MagicOnion.Client
{
    public interface IMagicOnionAwareGrpcChannel
    {
        /// <summary>
        /// Register the StreamingHub under the management of the channel.
        /// </summary>
        void ManageStreamingHubClient(IStreamingHubMarker streamingHub, Func<Task> disposeAsync, Task waitForDisconnect);

        /// <summary>
        /// Gets all StreamingHubs that depends on the channel.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<IStreamingHubMarker> GetAllManagedStreamingHubs();

        /// <summary>
        /// Create a <see cref="Grpc.Core.CallInvoker"/> from the channel.
        /// </summary>
        /// <returns></returns>
        CallInvoker CreateCallInvoker();
    }
}
