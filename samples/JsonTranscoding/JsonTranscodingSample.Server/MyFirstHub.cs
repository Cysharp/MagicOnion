using JsonTranscodingSample.Shared;
using MagicOnion.Server.Hubs;

namespace JsonTranscodingSample.Server;

// NOTE: JsonTranscoding is not supported for StreamingHub. JsonTranscoding will ignore the StreamingHub.
public class MyFirstHub : StreamingHubBase<IMyFirstHub, IMyFirstHubReceiver>, IMyFirstHub;
