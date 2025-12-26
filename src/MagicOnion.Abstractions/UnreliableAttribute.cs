namespace MagicOnion;

/// <summary>
/// Indicates that a StreamingHub method may be invoked over an unreliable transport such as UDP.
/// </summary>
/// <remarks>
/// StreamingHub methods marked with this attribute may be invoked over unreliable transports such as UDP, and therefore calls may be lost.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class UnreliableAttribute : Attribute;
