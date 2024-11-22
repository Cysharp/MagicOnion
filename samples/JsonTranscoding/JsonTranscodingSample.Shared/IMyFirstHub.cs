using MagicOnion;

namespace JsonTranscodingSample.Shared;

public interface IMyFirstHub : IStreamingHub<IMyFirstHub, IMyFirstHubReceiver>;
public interface IMyFirstHubReceiver;
