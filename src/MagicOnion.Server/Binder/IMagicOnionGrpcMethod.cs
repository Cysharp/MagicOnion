using System.Reflection;
using Grpc.Core;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionGrpcMethod
{
    MethodType MethodType { get; }
    Type ServiceType { get; }
    string ServiceName { get; }
    string MethodName { get; }
    MethodInfo MethodInfo { get; }
}

public interface IMagicOnionGrpcMethod<TService> : IMagicOnionGrpcMethod
    where TService : class
{
    void Bind(IMagicOnionGrpcMethodBinder<TService> binder);
}
