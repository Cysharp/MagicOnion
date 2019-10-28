using System;
using System.Text;
using MagicOnion.Server;

namespace MagicOnion.AspNetCore
{
    public class MagicOnionServiceDefinitionGlueDescriptor
    {
        public Type GlueServiceType { get; }
        public MagicOnionServiceDefinition ServiceDefinition { get; }

        public MagicOnionServiceDefinitionGlueDescriptor(Type glueServiceType, MagicOnionServiceDefinition serviceDefinition)
        {
            GlueServiceType = glueServiceType;
            ServiceDefinition = serviceDefinition;
        }
    }
}