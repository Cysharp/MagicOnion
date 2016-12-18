using Grpc.Core;
using System.Collections.Generic;

namespace MagicOnion.Server
{
    public class MagicOnionServiceDefinition
    {
        public ServerServiceDefinition ServerServiceDefinition { get; private set; }
        public IReadOnlyList<MethodHandler> MethodHandlers { get; private set; }

        public MagicOnionServiceDefinition(ServerServiceDefinition definition, IReadOnlyList<MethodHandler> handlers)
        {
            this.ServerServiceDefinition = definition;
            this.MethodHandlers = handlers;
        }

        public static implicit operator ServerServiceDefinition(MagicOnionServiceDefinition self)
        {
            return self.ServerServiceDefinition;
        }
    }
}
