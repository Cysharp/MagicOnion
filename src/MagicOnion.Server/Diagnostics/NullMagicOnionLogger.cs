using Grpc.Core;
using MagicOnion.Server.Hubs;

namespace MagicOnion.Server.Diagnostics;

public class NullMagicOnionLogger : IMagicOnionLogger
{
    public void BeginBuildServiceDefinition()
    {
    }

    public void BeginInvokeMethod(ServiceContext context, Type type)
    {
    }

    public void ReadFromStream(ServiceContext context, Type type, bool complete)
    {
    }

    public void WriteToStream(ServiceContext context, Type type)
    {
    }

    public void EndBuildServiceDefinition(double elapsed)
    {
    }

    public void EndInvokeMethod(ServiceContext context, Type type, double elapsed, bool isErrorOrInterrupted)
    {
    }

    public void BeginInvokeHubMethod(StreamingHubContext context, ReadOnlyMemory<byte> request, Type type)
    {
    }

    public void EndInvokeHubMethod(StreamingHubContext context, int responseSize, Type? type, double elapsed, bool isErrorOrInterrupted)
    {
    }

    public void InvokeHubBroadcast(string groupName, int responseSize, int broadcastGroupCount)
    {
    }

    public void Error(Exception ex, ServerCallContext context)
    {
    }
    public void Error(Exception ex, StreamingHubContext context)
    {
    }
}
