using System.Reflection;
using MagicOnion.Internal;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Client.Tests;

public class StreamingHubTransportModeResolverTest
{
    // Type.GetMethod on an interface does not return methods inherited from base interfaces, so search the whole hierarchy.
    static MethodInfo Method(Type type, string name)
        => new[] { type }.Concat(type.GetInterfaces())
            .Select(t => t.GetMethod(name))
            .FirstOrDefault(m => m is not null)
            ?? throw new InvalidOperationException($"{type.Name}.{name} not found");

    static StreamingHubTransportMode Resolve<TRoot>(string name)
        => StreamingHubTransportModeResolver.Resolve(typeof(TRoot), Method(typeof(TRoot), name));

    [Fact]
    public void NoAttribute_IsReliable()
    {
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<IMethodAttributeHub>(nameof(IMethodAttributeHub.PlainAsync)));
    }

    [Fact]
    public void MethodAttribute_ShorthandAndCanonical()
    {
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<IMethodAttributeHub>(nameof(IMethodAttributeHub.SequencedAsync)));
        Assert.Equal(StreamingHubTransportMode.UnreliableUnordered, Resolve<IMethodAttributeHub>(nameof(IMethodAttributeHub.UnorderedAsync)));
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<IMethodAttributeHub>(nameof(IMethodAttributeHub.ExplicitReliableAsync)));
        Assert.Equal(StreamingHubTransportMode.UnreliableUnordered, Resolve<IMethodAttributeHub>(nameof(IMethodAttributeHub.Canonical)));
    }

    [Fact]
    public void InterfaceDefault_AppliesToMethods_MethodOverrides()
    {
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<IUnreliableByDefaultHub>(nameof(IUnreliableByDefaultHub.MoveAsync)));
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<IUnreliableByDefaultHub>(nameof(IUnreliableByDefaultHub.JoinAsync)));
    }

    [Fact]
    public void MultipleAttributesOnMethod_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Resolve<IConflictingMethodHub>(nameof(IConflictingMethodHub.BothAsync)));
        Assert.Contains("multiple transport attributes", ex.Message);
    }

    [Fact]
    public void MultipleAttributesOnInterface_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Resolve<IConflictingInterfaceHub>(nameof(IConflictingInterfaceHub.MoveAsync)));
        Assert.Contains("multiple transport attributes", ex.Message);
    }

    [Fact]
    public void InheritedDefault_NearestToRootWins()
    {
        // IReliableBase ([Reliable]) <- IUnreliableMid ([Unreliable]) <- IRootWithoutAttribute
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<IRootWithoutAttribute>(nameof(IReliableBase.BaseAsync)));
        // Accessed directly through the base interface, its own default applies.
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<IReliableBase>(nameof(IReliableBase.BaseAsync)));
    }

    [Fact]
    public void RootAttribute_OverridesInheritedDefaults()
    {
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<IReliableRoot>(nameof(IReliableBase.BaseAsync)));
    }

    [Fact]
    public void UnrelatedInheritedDefault_DoesNotApply()
    {
        // ISideA declares A without attributes; ISideB is [Unreliable] and declares B.
        Assert.Equal(StreamingHubTransportMode.Reliable, Resolve<ITwoSidesRoot>(nameof(ISideA.AAsync)));
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<ITwoSidesRoot>(nameof(ISideB.BAsync)));
    }

    [Fact]
    public void Diamond_ConflictingDefaultsAtSameDistance_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Resolve<IDiamondConflict>(nameof(IDiamondBase.DAsync)));
        Assert.Contains("ambiguous", ex.Message);
    }

    [Fact]
    public void Diamond_SameDefaultsAtSameDistance_Resolves()
    {
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<IDiamondAgree>(nameof(IDiamondBase.DAsync)));
    }

    [Fact]
    public void Diamond_OneSideWithoutDefault_Resolves()
    {
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, Resolve<IDiamondOneSide>(nameof(IDiamondBase.DAsync)));
    }

    [Fact]
    public void Resolve_MethodNotMemberOfRoot_Throws()
    {
        Assert.Throws<ArgumentException>(() => StreamingHubTransportModeResolver.Resolve(typeof(IMethodAttributeHub), Method(typeof(ISideA), nameof(ISideA.AAsync))));
    }

    [Fact]
    public void ValidateHubMethod_UnreliableWithResult_Throws()
    {
        var type = typeof(IResultReturningHub);
        Assert.Throws<InvalidOperationException>(() => StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.ValueTaskOfTAsync)), StreamingHubTransportMode.UnreliableSequenced));
        Assert.Throws<InvalidOperationException>(() => StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.TaskOfTAsync)), StreamingHubTransportMode.UnreliableUnordered));

        // Allowed shapes.
        StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.VoidMethod)), StreamingHubTransportMode.UnreliableSequenced);
        StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.TaskAsync)), StreamingHubTransportMode.UnreliableSequenced);
        StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.ValueTaskAsync)), StreamingHubTransportMode.UnreliableUnordered);
        // Reliable methods may return results.
        StreamingHubTransportModeResolver.ValidateHubMethod(Method(type, nameof(IResultReturningHub.ValueTaskOfTAsync)), StreamingHubTransportMode.Reliable);
    }

    [Fact]
    public void ValidateReceiverMethod_UnreliableClientResult_Throws()
    {
        var type = typeof(IClientResultReceiver);
        Assert.Throws<InvalidOperationException>(() => StreamingHubTransportModeResolver.ValidateReceiverMethod(Method(type, nameof(IClientResultReceiver.OnResultAsync)), StreamingHubTransportMode.UnreliableSequenced));
        StreamingHubTransportModeResolver.ValidateReceiverMethod(Method(type, nameof(IClientResultReceiver.OnNotify)), StreamingHubTransportMode.UnreliableSequenced);
        StreamingHubTransportModeResolver.ValidateReceiverMethod(Method(type, nameof(IClientResultReceiver.OnResultAsync)), StreamingHubTransportMode.Reliable);
    }

    [Fact]
    public void Contract_ForHub_SortedByMethodIdAndExcludesStreamingHubMembers()
    {
        var contract = StreamingHubTransportContract.ForHub<IContractHub>();

        Assert.Equal(3, contract.Methods.Count);
        Assert.True(contract.HasUnreliableMethods);
        for (var i = 1; i < contract.Methods.Count; i++)
        {
            Assert.True(contract.Methods[i - 1].MethodId < contract.Methods[i].MethodId);
        }

        Assert.True(contract.TryGetMode(5, out var explicitIdMode));
        Assert.Equal(StreamingHubTransportMode.UnreliableUnordered, explicitIdMode);
        Assert.True(contract.TryGetMode(FNV1A32.GetHashCode(nameof(IContractHub.SequencedAsync)), out var sequencedMode));
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, sequencedMode);
        Assert.True(contract.TryGetMode(FNV1A32.GetHashCode(nameof(IContractHub.ReliableAsync)), out var reliableMode));
        Assert.Equal(StreamingHubTransportMode.Reliable, reliableMode);
        Assert.False(contract.TryGetMode(FNV1A32.GetHashCode("FireAndForget"), out _));
    }

    [Fact]
    public void Contract_ForReceiver_ResolvesModes()
    {
        var contract = StreamingHubTransportContract.ForReceiver<IContractReceiver>();

        Assert.Equal(2, contract.Methods.Count);
        Assert.True(contract.TryGetMode(FNV1A32.GetHashCode(nameof(IContractReceiver.OnPosition)), out var position));
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, position);
        Assert.True(contract.TryGetMode(FNV1A32.GetHashCode(nameof(IContractReceiver.OnJoinedAsync)), out var joined));
        Assert.Equal(StreamingHubTransportMode.Reliable, joined);
    }

    [Fact]
    public void Contract_IsCachedPerClosedGenericType()
    {
        var first = StreamingHubTransportContract.ForHub<IGenericHub<int>>();
        var second = StreamingHubTransportContract.ForHub<IGenericHub<int>>();
        var other = StreamingHubTransportContract.ForHub<IGenericHub<string>>();

        Assert.Same(first, second);
        Assert.NotSame(first, other);
        Assert.True(first.TryGetMode(FNV1A32.GetHashCode(nameof(IGenericHub<int>.UpdateAsync)), out var mode));
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, mode);
    }

    [Fact]
    public void UnreliableMethods_ContainsOnlyUnreliableMethodsSortedByMethodId()
    {
        var contract = StreamingHubTransportContract.ForHub<IContractHub>();
        var table = contract.UnreliableMethods;

        Assert.Equal(2, table.Count);
        Assert.Equal(new[]
        {
            new StreamingHubTransportModeEntry(5, StreamingHubTransportMode.UnreliableUnordered),
            new StreamingHubTransportModeEntry(FNV1A32.GetHashCode(nameof(IContractHub.SequencedAsync)), StreamingHubTransportMode.UnreliableSequenced),
        }.OrderBy(x => x.MethodId), table.Entries);
        Assert.False(table.TryGetMode(FNV1A32.GetHashCode(nameof(IContractHub.ReliableAsync)), out _));

        var reliableOnly = StreamingHubTransportContract.ForHub<IReliableOnlyHub>();
        Assert.False(reliableOnly.HasUnreliableMethods);
        Assert.Same(StreamingHubTransportModeTable.Empty, reliableOnly.UnreliableMethods);
    }

    [Fact]
    public void Agree_PeerHasAdditionalMethods_KeepsCommonMethods()
    {
        // Rolling upgrade: the server (v2) added an unreliable method and a reliable method; the client (v1) does not know them.
        var v1 = StreamingHubTransportContract.ForHub<IPositionHubV1>().UnreliableMethods;
        var v2 = StreamingHubTransportContract.ForHub<IPositionHubV2>().UnreliableMethods;

        var agreedOnServer = StreamingHubTransportModeTable.Agree(v2, v1, out var serverConflicts);
        var agreedOnClient = StreamingHubTransportModeTable.Agree(v1, v2, out var clientConflicts);

        Assert.True(agreedOnServer.ContentEquals(v1));
        Assert.True(agreedOnClient.ContentEquals(v1));
        Assert.Empty(serverConflicts);
        Assert.Empty(clientConflicts);
        Assert.True(agreedOnServer.TryGetMode(FNV1A32.GetHashCode(nameof(IPositionHubV1.MoveAsync)), out var mode));
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, mode);
        Assert.False(agreedOnServer.TryGetMode(FNV1A32.GetHashCode(nameof(IPositionHubV2.EmoteAsync)), out _));
    }

    [Fact]
    public void Agree_ModeConflict_ExcludesMethodAndReportsConflict()
    {
        var v1 = StreamingHubTransportContract.ForHub<IPositionHubV1>().UnreliableMethods;
        var changed = StreamingHubTransportContract.ForHub<IPositionHubV2ModeChanged>().UnreliableMethods;

        var agreed = StreamingHubTransportModeTable.Agree(v1, changed, out var conflicts);

        Assert.Same(StreamingHubTransportModeTable.Empty, agreed);
        var conflict = Assert.Single(conflicts);
        Assert.Equal(FNV1A32.GetHashCode(nameof(IPositionHubV1.MoveAsync)), conflict.MethodId);
        Assert.Equal(StreamingHubTransportMode.UnreliableSequenced, conflict.LocalMode);
        Assert.Equal(StreamingHubTransportMode.UnreliableUnordered, conflict.PeerMode);
    }

    [Fact]
    public void Agree_NoCommonMethods_IsEmptyWithoutConflicts()
    {
        var v1 = StreamingHubTransportContract.ForHub<IPositionHubV1>().UnreliableMethods;
        var reliableOnly = StreamingHubTransportContract.ForHub<IReliableOnlyHub>().UnreliableMethods;

        var agreed = StreamingHubTransportModeTable.Agree(v1, reliableOnly, out var conflicts);

        Assert.Same(StreamingHubTransportModeTable.Empty, agreed);
        Assert.Empty(conflicts);
    }

    [Fact]
    public void IsSupersetOf_VerifiesAgreedTableAgainstLocalContract()
    {
        var v1 = StreamingHubTransportContract.ForHub<IPositionHubV1>().UnreliableMethods;
        var v2 = StreamingHubTransportContract.ForHub<IPositionHubV2>().UnreliableMethods;
        var changed = StreamingHubTransportContract.ForHub<IPositionHubV2ModeChanged>().UnreliableMethods;
        var agreed = StreamingHubTransportModeTable.Agree(v1, v2);

        Assert.True(v1.IsSupersetOf(agreed));
        Assert.True(v2.IsSupersetOf(agreed));
        Assert.True(v1.IsSupersetOf(StreamingHubTransportModeTable.Empty));
        Assert.False(v1.IsSupersetOf(v2));
        Assert.False(v1.IsSupersetOf(changed));
    }

    [Fact]
    public void ModeTable_Create_SortsEntriesAndRejectsInvalidInput()
    {
        var table = StreamingHubTransportModeTable.Create(new[]
        {
            new StreamingHubTransportModeEntry(10, StreamingHubTransportMode.UnreliableUnordered),
            new StreamingHubTransportModeEntry(-5, StreamingHubTransportMode.UnreliableSequenced),
            new StreamingHubTransportModeEntry(3, StreamingHubTransportMode.UnreliableSequenced),
        });
        Assert.Equal(new[] { -5, 3, 10 }, table.Entries.Select(x => x.MethodId));

        Assert.Throws<ArgumentException>(() => StreamingHubTransportModeTable.Create(new[]
        {
            new StreamingHubTransportModeEntry(1, StreamingHubTransportMode.UnreliableSequenced),
            new StreamingHubTransportModeEntry(1, StreamingHubTransportMode.UnreliableSequenced),
        }));
        Assert.Throws<ArgumentException>(() => StreamingHubTransportModeTable.Create(new[]
        {
            new StreamingHubTransportModeEntry(1, StreamingHubTransportMode.Reliable),
        }));
        Assert.Throws<ArgumentException>(() => StreamingHubTransportModeTable.Create(new[]
        {
            new StreamingHubTransportModeEntry(1, (StreamingHubTransportMode)99),
        }));
    }

    [Fact]
    public void Contract_InvalidUnreliableResultMethod_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => StreamingHubTransportContract.ForHub<IInvalidResultHub>());
        Assert.Throws<InvalidOperationException>(() => StreamingHubTransportContract.ForReceiver<IInvalidClientResultReceiver>());
    }

    [Fact]
    public void Contract_InvalidContract_RethrowsOriginalExceptionOnEveryAccess()
    {
        // The generic cache must not surface a TypeInitializationException or swallow the message on later accesses.
        var first = Assert.Throws<InvalidOperationException>(() => StreamingHubTransportContract.ForHub<IInvalidResultHub>());
        var second = Assert.Throws<InvalidOperationException>(() => StreamingHubTransportContract.ForHub<IInvalidResultHub>());

        Assert.Contains("must return void, Task, or ValueTask", first.Message);
        Assert.Equal(first.Message, second.Message);
    }

    [Fact]
    public void Contract_DuplicateMethodId_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => StreamingHubTransportContract.ForHub<IDuplicateMethodIdHub>());
        Assert.Contains("duplicate MethodId", ex.Message);
    }

    // Fixtures

    public interface IMethodAttributeHub : IStreamingHub<IMethodAttributeHub, IContractReceiver>
    {
        Task PlainAsync();
        [Unreliable] ValueTask SequencedAsync(int value);
        [UnreliableUnordered] Task UnorderedAsync();
        [Reliable] ValueTask<int> ExplicitReliableAsync();
        [Transport(StreamingHubTransportMode.UnreliableUnordered)] void Canonical();
    }

    [Unreliable]
    public interface IUnreliableByDefaultHub : IStreamingHub<IUnreliableByDefaultHub, IContractReceiver>
    {
        Task MoveAsync(int x);
        [Reliable] ValueTask<int> JoinAsync(string name);
    }

    public interface IConflictingMethodHub
    {
        [Unreliable, Reliable] Task BothAsync();
    }

    [Unreliable]
    [Transport(StreamingHubTransportMode.Reliable)]
    public interface IConflictingInterfaceHub
    {
        Task MoveAsync();
    }

    [Reliable]
    public interface IReliableBase
    {
        Task BaseAsync();
    }

    [Unreliable]
    public interface IUnreliableMid : IReliableBase
    {
    }

    public interface IRootWithoutAttribute : IUnreliableMid
    {
    }

    [Reliable]
    public interface IReliableRoot : IUnreliableMid
    {
    }

    public interface ISideA
    {
        Task AAsync();
    }

    [Unreliable]
    public interface ISideB
    {
        Task BAsync();
    }

    public interface ITwoSidesRoot : ISideA, ISideB
    {
    }

    public interface IDiamondBase
    {
        Task DAsync();
    }

    [Unreliable]
    public interface IDiamondLeft : IDiamondBase
    {
    }

    [Reliable]
    public interface IDiamondRightReliable : IDiamondBase
    {
    }

    [Unreliable]
    public interface IDiamondRightUnreliable : IDiamondBase
    {
    }

    public interface IDiamondRightNone : IDiamondBase
    {
    }

    public interface IDiamondConflict : IDiamondLeft, IDiamondRightReliable
    {
    }

    public interface IDiamondAgree : IDiamondLeft, IDiamondRightUnreliable
    {
    }

    public interface IDiamondOneSide : IDiamondLeft, IDiamondRightNone
    {
    }

    public interface IResultReturningHub
    {
        void VoidMethod();
        Task TaskAsync();
        ValueTask ValueTaskAsync();
        Task<int> TaskOfTAsync();
        ValueTask<int> ValueTaskOfTAsync();
    }

    public interface IClientResultReceiver
    {
        void OnNotify(int value);
        Task<int> OnResultAsync(int value);
    }

    public interface IContractHub : IStreamingHub<IContractHub, IContractReceiver>
    {
        [Unreliable] Task SequencedAsync(int value);
        ValueTask<int> ReliableAsync();
        [UnreliableUnordered, MethodId(5)] Task ExplicitIdAsync();
    }

    public interface IContractReceiver
    {
        [Unreliable] void OnPosition(int x, int y);
        Task<bool> OnJoinedAsync(string name);
    }

    public interface IGenericHub<T> : IStreamingHub<IGenericHub<T>, IContractReceiver>
    {
        [Unreliable] Task UpdateAsync(T value);
    }

    // Rolling-upgrade fixtures: v1 is the contract an older client still runs; v2 is the updated server contract.
    public interface IPositionHubV1
    {
        [Unreliable] Task MoveAsync(int x, int y);
    }

    public interface IPositionHubV2
    {
        [Unreliable] Task MoveAsync(int x, int y);
        [UnreliableUnordered] Task EmoteAsync(int id);
        Task<int> QueryAsync();
    }

    public interface IPositionHubV2ModeChanged
    {
        [UnreliableUnordered] Task MoveAsync(int x, int y);
    }

    public interface IReliableOnlyHub
    {
        Task MoveAsync(int x, int y);
    }

    public interface IInvalidResultHub
    {
        [Unreliable] Task<int> BadAsync();
    }

    public interface IInvalidClientResultReceiver
    {
        [Unreliable] Task<int> OnBadAsync();
    }

    public interface IDuplicateMethodIdHub
    {
        [MethodId(1)] Task XAsync();
        [MethodId(1)] Task YAsync();
    }
}
