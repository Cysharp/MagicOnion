using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.CodeAnalysis
{
    public enum MethodType
    {
        Unary = 0,
        ClientStreaming = 1,
        ServerStreaming = 2,
        DuplexStreaming = 3
    }

    public class InterfaceDefintion
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public MethodDefinition[] Methods { get; set; }
    }

    public class MethodDefinition
    {
        public MethodType MethodType { get; set; }
        public string RequestType { get; set; }
        public string ResponseType { get; set; }
        public ParameterDefinition[] Parameters { get; set; }
    }

    public class ParameterDefinition
    {
        public string TypeName { get; set; }
        public string ParameterName { get; set; }
    }
}
