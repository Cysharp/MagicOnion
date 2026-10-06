using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server.Hubs;

// Shared contract fixtures for verifying that the source generator (Roslyn) and the runtime (Reflection)
// resolve the same transport mode for every method. Keep every contract here valid.
namespace SampleServiceDefinitions.Hubs
{
    public interface ITransportModeReceiver
    {
        [Unreliable]
        void OnMove(int x, int y);
        void OnJoined(string name);
        Task<int> OnQueryAsync(int value);
    }

    public interface ITransportModeHub : IStreamingHub<ITransportModeHub, ITransportModeReceiver>
    {
        [Unreliable]
        Task MoveAsync(int x, int y);
        [UnreliableUnordered]
        ValueTask EmoteAsync(int id);
        [Transport(StreamingHubTransportMode.UnreliableSequenced)]
        void Ping();
        Task<int> QueryAsync(int value);
        Task JoinAsync(string name);
        [Unreliable, MethodId(777)]
        Task ExplicitIdAsync();
    }

    [Unreliable]
    public interface ITransportModeDefaultReceiver
    {
        void OnMove(int x);
        [Reliable]
        void OnJoined(string name);
    }

    [Unreliable]
    public interface ITransportModeDefaultHub : IStreamingHub<ITransportModeDefaultHub, ITransportModeDefaultReceiver>
    {
        Task MoveAsync(int x);
        [Reliable]
        Task<int> JoinAsync(string name);
    }

    [Reliable]
    public interface ITransportModeBase
    {
        Task BaseAsync();
    }

    [Unreliable]
    public interface ITransportModeMiddle : ITransportModeBase
    {
        Task MiddleAsync();
    }

    public interface ITransportModeSide
    {
        Task SideAsync();
    }

    public interface ITransportModeInheritedHub : IStreamingHub<ITransportModeInheritedHub, ITransportModeReceiver>, ITransportModeMiddle, ITransportModeSide
    {
        Task<int> RootAsync();
    }

    public interface ITransportModeDiamondBase
    {
        Task DiamondAsync();
    }

    [Unreliable]
    public interface ITransportModeDiamondLeft : ITransportModeDiamondBase
    {
    }

    [Unreliable]
    public interface ITransportModeDiamondRight : ITransportModeDiamondBase
    {
    }

    public interface ITransportModeDiamondHub : IStreamingHub<ITransportModeDiamondHub, ITransportModeReceiver>, ITransportModeDiamondLeft, ITransportModeDiamondRight
    {
    }
}
