using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion
{
    /// <summary>
    /// instruction for moc.exe, surround #if symbol with output code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Obsolete("GenerateDefineDebug is no longer used from the generator. Use C# '#if' directive instead.")]
    public class GenerateDefineDebugAttribute : Attribute
    {
        public GenerateDefineDebugAttribute()
        {
        }
    }
}
