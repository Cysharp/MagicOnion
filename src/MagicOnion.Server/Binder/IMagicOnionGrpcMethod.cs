using System.Reflection;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionGrpcMethod;

public interface IMagicOnionGrpcMethod<TService> : IMagicOnionGrpcMethod
    where TService : class
{
    string ServiceName { get; }
    string MethodName { get; }
    MethodInfo MethodInfo { get; }
    void Bind(IMagicOnionGrpcMethodBinder<TService> binder);
}
