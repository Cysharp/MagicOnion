using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Hosting
{
    public class MagicOnionHostedServiceDefinition
    {
        public string Name { get; }

        public MagicOnionServiceDefinition ServiceDefinition { get; }

        public MagicOnionHostedServiceDefinition(string name, MagicOnionServiceDefinition serviceDefinition)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (serviceDefinition == null) throw new ArgumentNullException(nameof(serviceDefinition));

            Name = name;
            ServiceDefinition = serviceDefinition;
        }
    }
}
