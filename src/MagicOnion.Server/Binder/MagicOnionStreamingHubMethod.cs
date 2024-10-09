using System.Reflection;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionStreamingHubMethod
{
    string ServiceName { get; }
    string MethodName { get; }

    [Obsolete]
    MethodInfo Method { get; }
}

// TODO:
public class MagicOnionStreamingHubMethod<TService, TRequest, TResponse>(string serviceName, string methodName, MethodInfo methodInfo) : IMagicOnionStreamingHubMethod
{
    public string ServiceName => serviceName;
    public string MethodName => methodName;
    public MethodInfo Method => methodInfo;

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, TRequest, ValueTask<TResponse>> invoker) : this(serviceName, methodName, invoker.Method)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, TRequest, Task<TResponse>> invoker) : this(serviceName, methodName, invoker.Method)
    {
    }
}

public class MagicOnionStreamingHubMethod<TService, TRequest>(string serviceName, string methodName, MethodInfo methodInfo) : IMagicOnionStreamingHubMethod
{
    public string ServiceName => serviceName;
    public string MethodName => methodName;
    public MethodInfo Method => methodInfo;

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, TRequest, ValueTask> invoker) : this(serviceName, methodName, invoker.Method)
    {
    }

    public MagicOnionStreamingHubMethod(string serviceName, string methodName, Func<TService, TRequest, Task> invoker) : this(serviceName, methodName, invoker.Method)
    {
    }
}

