using Grpc.Core;
using MagicOnion.Server.Hubs;
using System.Collections.Generic;

namespace MagicOnion.Server
{
    public class MagicOnionServiceDefinition
    {
        public ServerServiceDefinition ServerServiceDefinition { get; private set; }
        public IReadOnlyList<MethodHandler> MethodHandlers { get; private set; }
        public IReadOnlyList<StreamingHubHandler> StreamingHubHandlers { get; private set; }

        public MagicOnionServiceDefinition(ServerServiceDefinition definition, IReadOnlyList<MethodHandler> handlers, IReadOnlyList<StreamingHubHandler> streamingHubHandlers)
        {
            this.ServerServiceDefinition = definition;
            this.MethodHandlers = handlers;
            this.StreamingHubHandlers = streamingHubHandlers;
        }

        public static implicit operator ServerServiceDefinition(MagicOnionServiceDefinition self)
        {
            return self.ServerServiceDefinition;
        }
    }
}
