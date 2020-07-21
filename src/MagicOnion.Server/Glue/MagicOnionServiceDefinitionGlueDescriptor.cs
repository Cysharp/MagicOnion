using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Server.Glue
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
