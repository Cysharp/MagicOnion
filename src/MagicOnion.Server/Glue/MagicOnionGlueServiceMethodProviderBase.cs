using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace MagicOnion.Server.Glue
{
    internal class MagicOnionGlueServiceMethodProviderBase
    {
        protected ServerServiceDefinition ServerServiceDefinition { get; }

        protected MagicOnionGlueServiceMethodProviderBase(ServerServiceDefinition serverServiceDefinition)
        {
            ServerServiceDefinition = serverServiceDefinition;
        }
    }
}
