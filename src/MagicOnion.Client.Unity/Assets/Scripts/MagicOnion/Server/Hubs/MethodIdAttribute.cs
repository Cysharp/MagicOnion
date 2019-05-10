using System;
using System.Collections.Generic;
using System.Text;

namespace MagicOnion.Server.Hubs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MethodIdAttribute : Attribute
    {
        public readonly int MethodId;

        public MethodIdAttribute(int methodId)
        {
            MethodId = methodId;
        }
    }
}
