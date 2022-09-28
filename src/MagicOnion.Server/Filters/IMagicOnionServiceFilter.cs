namespace MagicOnion.Server.Filters;

/// <summary>
/// An filter that surrounds execution of the Unary service method.
/// </summary>
public interface IMagicOnionServiceFilter : IMagicOnionFilterMetadata
{
    ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next);
}