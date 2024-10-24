#if !NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
    internal class RequiresUnreferencedCodeAttribute : Attribute
    {
        public string Message { get; }
        public string? Url { get; }

        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }
    }
}
#endif
