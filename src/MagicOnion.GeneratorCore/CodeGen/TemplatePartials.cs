using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Generator.CodeAnalysis;

namespace MagicOnion.Generator.CodeGen
{
    public partial class CodeTemplate
    {
        public string Namespace { get; set; }
        public IReadOnlyList<InterfaceDefinition> Interfaces { get; set; }
    }

    public partial class HubTemplate
    {
        public string Namespace { get; set; }
        public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; set; }
    }

    public partial class RegisterTemplate
    {
        public string Namespace { get; set; }
        public bool OmitUnityAttribute { get; set; }
        public IReadOnlyList<MagicOnionServiceInfo> Services { get; set; }
        public IReadOnlyList<MagicOnionStreamingHubInfo> Hubs { get; set; }
    }

    public partial class ResolverTemplate
    {
        public string Namespace;
        public string FormatterNamespace { get; set; }
        public string ResolverName = "GeneratedResolver";
        public IReadOnlyList<IResolverRegisterInfo> RegisterInfos;
    }

    public partial class EnumTemplate
    {
        public string Namespace;
        public IReadOnlyList<EnumSerializationInfo> EnumSerializationInfos;
    }
}