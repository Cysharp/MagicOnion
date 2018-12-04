using MagicOnion.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.Generator
{
    public partial class CodeTemplate
    {
        public string Namespace { get; set; }
        public MagicOnion.CodeAnalysis.InterfaceDefinition[] Interfaces { get; set; }
    }

    public partial class HubTemplate
    {
        public string Namespace { get; set; }
        public (InterfaceDefinition hubDef, InterfaceDefinition receiverDef)[] Interfaces { get; set; }
    }

    public partial class RegisterTemplate
    {
        public string Namespace { get; set; }
        public bool UnuseUnityAttribute { get; set; }
        public MagicOnion.CodeAnalysis.InterfaceDefinition[] Interfaces { get; set; }
    }

    public partial class ResolverTemplate
    {
        public string Namespace;
        public string FormatterNamespace { get; set; }
        public string ResolverName = "GeneratedResolver";
        public IResolverRegisterInfo[] registerInfos;
    }
    public partial class EnumTemplate
    {
        public string Namespace;
        public EnumSerializationInfo[] enumSerializationInfos;
    }
}