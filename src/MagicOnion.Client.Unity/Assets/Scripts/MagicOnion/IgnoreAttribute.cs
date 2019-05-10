using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion
{
    /// <summary>
    /// Don't register on MagicOnionEngine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class,AllowMultiple = false,Inherited = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}
