using Grpc.Core;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Diagnostics;

public interface IMagicOnionLogger
{
    void BeginBuildServiceDefinition();
    void EndBuildServiceDefinition(double elapsed);

    void BeginInvokeMethod(ServiceContext context, Type type);
    void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted);

    void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type);
    void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted);
    void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount);

    void WriteToStream(ServiceContext context, Type type);
    void ReadFromStream(ServiceContext context, Type type, bool complete);

    void Error(Exception ex, ServerCallContext context);
    void Error(Exception ex, StreamingHubContext context);
}
