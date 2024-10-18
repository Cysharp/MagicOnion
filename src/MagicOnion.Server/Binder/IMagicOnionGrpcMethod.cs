using System.Reflection;
using Grpc.Core;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server.Binder;

public interface IMagicOnionGrpcMethod
{
    MethodType MethodType { get; }
    Type ServiceImplementationType { get; }
    string ServiceName { get; }
    string MethodName { get; }
    MethodHandlerMetadata Metadata { get; }
}

public interface IMagicOnionGrpcMethod<TService> : IMagicOnionGrpcMethod
    where TService : class
{
    void Bind(IMagicOnionGrpcMethodBinder<TService> binder);
}
