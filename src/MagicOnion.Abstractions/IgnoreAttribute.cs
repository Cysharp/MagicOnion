namespace MagicOnion;

/// <summary>
/// Don't register on MagicOnionEngine.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false,Inherited = false)]
public class IgnoreAttribute : Attribute
{
}
