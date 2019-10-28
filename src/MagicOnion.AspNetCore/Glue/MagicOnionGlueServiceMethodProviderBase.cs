using System;
using System.Collections.Generic;
using Grpc.Core;

namespace MagicOnion.AspNetCore
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