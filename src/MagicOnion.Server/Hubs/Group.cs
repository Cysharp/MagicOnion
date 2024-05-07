using Multicaster;

namespace MagicOnion.Server.Hubs;

public interface IGroup<T> : IMulticastAsyncGroup<T>
{
    ValueTask RemoveAsync(ServiceContext context);
}

internal class Group<T> : IGroup<T>
{
    readonly IMulticastAsyncGroup<T> _group;

    public Group(IMulticastAsyncGroup<T> group)
    {
        _group = group;
    }

    public T All
        => _group.All;

    public T Except(IReadOnlyList<Guid> excludes)
        => _group.Except(excludes);

    public T Only(IReadOnlyList<Guid> targets)
        => _group.Only(targets);

    public ValueTask RemoveAsync(ServiceContext context)
        => _group.RemoveAsync(context.ContextId);

    public ValueTask AddAsync(Guid key, T receiver)
        => _group.AddAsync(key, receiver);

    public ValueTask RemoveAsync(Guid key)
        => _group.RemoveAsync(key);

    public ValueTask<int> CountAsync()
        => _group.CountAsync();
}
