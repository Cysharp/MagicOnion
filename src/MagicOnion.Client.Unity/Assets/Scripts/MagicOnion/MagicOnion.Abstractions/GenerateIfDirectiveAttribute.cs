using System;

namespace MagicOnion
{
    /// <summary>
    /// instruction for moc.exe, surround #if symbol with output code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Obsolete("GenerateIfDirective is no longer used from the generator. Use C# '#if' directive instead.")]
    public class GenerateIfDirectiveAttribute : Attribute
    {
        public GenerateIfDirectiveAttribute(string condition)
        {
        }
    }
}
