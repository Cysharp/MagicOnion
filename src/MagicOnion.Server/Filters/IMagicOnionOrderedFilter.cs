namespace MagicOnion.Server.Filters;

/// <summary>
/// An interface that provides filter order.
/// </summary>
public interface IMagicOnionOrderedFilter : IMagicOnionFilterMetadata
{
    int Order { get; }
}