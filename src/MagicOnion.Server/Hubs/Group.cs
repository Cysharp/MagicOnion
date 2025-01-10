using System;
using System.Collections.Immutable;
using Cysharp.Runtime.Multicast;

namespace MagicOnion.Server.Hubs;

public interface IGroup<T> : IMulticastGroup<Guid, T>
{
    ValueTask RemoveAsync(ServiceContext context);
    ValueTask<int> CountAsync();
}

internal class Group<T> : IGroup<T>
{
    readonly IMulticastAsyncGroup<Guid, T> group;

    internal string Name { get; }

    public Group(IMulticastAsyncGroup<Guid, T> group, string name)
    {
        this.group = group;
        this.Name = name;
    }

    public T All
        => group.All;

    public T Except(IEnumerable<Guid> excludes)
        => group.Except(excludes);

    public T Only(IEnumerable<Guid> targets)
        => group.Only(targets);

    public T Single(Guid target)
        => group.Single(target);

    public ValueTask RemoveAsync(ServiceContext context)
        => group.RemoveAsync(context.ContextId);

    public ValueTask<int> CountAsync()
        => group.CountAsync();
}
