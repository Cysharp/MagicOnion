using System.Collections.Immutable;
using Cysharp.Runtime.Multicast;

namespace MagicOnion.Server.Hubs;

public interface IGroup<T> : IMulticastGroup<T>
{
    ValueTask RemoveAsync(ServiceContext context);
    ValueTask<int> CountAsync();
}

internal class Group<T> : IGroup<T>
{
    readonly IMulticastAsyncGroup<T> group;

    internal string Name { get; }

    public Group(IMulticastAsyncGroup<T> group, string name)
    {
        this.group = group;
        this.Name = name;
    }

    public T All
        => group.All;

    public T Except(ImmutableArray<Guid> excludes)
        => group.Except(excludes);

    public T Only(ImmutableArray<Guid> targets)
        => group.Only(targets);

    public T Single(Guid target)
        => group.Single(target);

    public ValueTask RemoveAsync(ServiceContext context)
        => group.RemoveAsync(context.ContextId);

    public ValueTask<int> CountAsync()
        => group.CountAsync();
}
