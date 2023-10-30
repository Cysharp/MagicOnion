using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MagicOnion.Internal
{
    /// <summary>
    /// Provide a dummy Null object to cheat grpc-dotnet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Grpc.Net (grpc-dotnet) does not allow null values as messages.
    /// However, gRPC does not need to know the contents of the message, and MagicOnion natively handles CLR objects.
    /// so there is no problem if a null object is encountered between implementation and serialization.
    /// </para>
    /// <para>
    /// See: https://github.com/grpc/grpc-dotnet/blob/51ec4d05e6b38532c959018728277f2477cc6a7e/src/Grpc.AspNetCore.Server/Internal/CallHandlers/UnaryServerCallHandler.cs#L52-L56
    /// </para>
    /// <para>
    /// grpc-dotnet also does not care about the message content,
    /// MagicOnion will replace the null value with a singleton-dummy `System.Object` instance as `T` by `Unsafe.As`.
    /// When serializing or deserializing a request/response, it will replace the dummy object back to `null` or with a dummy object.
    /// </para>
    /// </remarks>
    /// <code>
    /// - Request (on server):
    ///     [MagicOnion Client]
    ///               |
    ///               | (MessagePack binary)
    ///               |
    ///     [ASP.NET Core gRPC server (grpc-dotnet)]
    ///               |
    ///     [MessageSerializer.Deserialize&lt;T> (MagicOnion)]
    ///               |
    ///               | (object or null)
    ///               |
    ///     [DangerousDummyNull.GetObjectOrDummyNull (MagicOnion)]
    ///               |
    ///               | (object or DummyNull)
    ///               |
    ///     [CallHandler (grpc-dotnet)]
    ///               |
    ///     [DangerousDummyNull.GetObjectOrDefault (MagicOnion)]
    ///               |
    ///               | (object or null)
    ///               |
    ///     [Unary method (User-code)]
    ///
    ///
    /// - Response (on server):
    ///     [Unary method (User-code)]
    ///               |
    ///               | (object or null)
    ///               |
    ///     [DangerousDummyNull.GetObjectOrDummyNull (MagicOnion)]
    ///               |
    ///               | (object or DummyNull)
    ///               |
    ///     [CallHandler (grpc-dotnet)]
    ///               |
    ///     [DangerousDummyNull.GetObjectOrDefault (MagicOnion)]
    ///               |
    ///               | (object or null)
    ///               |
    ///     [MessageSerializer.Serialize&lt;T> (MagicOnion)]
    ///               |
    ///               | (MessagePack binary)
    ///               |
    ///     [ASP.NET Core gRPC server (grpc-dotnet)]
    ///               |
    ///     [MagicOnion Client]
    /// </code>
    internal class DangerousDummyNull
    {
        public static DangerousDummyNull Instance { get; } = new DangerousDummyNull();

        DangerousDummyNull()
        {}

        public static T GetObjectOrDummyNull<T>(T value)
        {
            if (value is null)
            {
                Debug.Assert(typeof(T).IsClass);
                var instance = Instance;
                return Unsafe.As<DangerousDummyNull, T>(ref instance);
            }

            return value;
        }

        public static T GetObjectOrDefault<T>(object value)
        {
            if (object.ReferenceEquals(value, Instance))
            {
                return default(T)!;
            }

            return (T)value;
        }
    }
}

