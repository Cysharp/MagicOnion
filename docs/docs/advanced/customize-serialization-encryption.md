# Customizing message serialization and encryption
MagicOnion uses MessagePack for serialization by default, but it also provides extension points to customize serialization.

It allows for customization, such as encryption and the using of serializers other than MessagePack.

## How to customize

Customizing serialization involves implementing a serializer that implements the `IMagicOnionSerializerProvider` interface and the `IMagicOnionSerializer` interface that the `Create` method returns.

You can use the implemented serializer provider by setting it to the `MagicOnionSerializerProvider.Default` property or passing it as an argument to `MagicOnionClient` and `StramingHubClient`.

## API
```csharp
/// <summary>
/// Provides a serializer for request/response of MagicOnion services and hub methods.
/// </summary>
public interface IMagicOnionSerializerProvider
{
    IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo);
}

/// <summary>
/// Provides a processing for message serialization.
/// </summary>
public interface IMagicOnionSerializer
{
    void Serialize<T>(IBufferWriter<byte> writer, in T? value);
    T? Deserialize<T>(in ReadOnlySequence<byte> bytes);
}

public static class MagicOnionSerializerProvider
{
    /// <summary>
    /// Gets or sets the <see cref="IMagicOnionSerializerProvider"/> to be used by default.
    /// </summary>
    public static IMagicOnionSerializerProvider Default { get; set; } = MessagePackMagicOnionSerializerProvider.Default;
}
```

## Example code
The following code is a simple example of performing XOR encryption:

```csharp
public class XorMessagePackMagicOnionSerializerProvider : IMagicOnionSerializerProvider
{
    const int MagicNumber = 0x11;

    readonly MessagePackSerializerOptions serializerOptions;

    public static IMagicOnionSerializerProvider Instance { get; } = new XorMessagePackMagicOnionSerializerProvider(MessagePackSerializer.DefaultOptions);

    XorMessagePackMagicOnionSerializerProvider(MessagePackSerializerOptions serializerOptions)
        => this.serializerOptions = serializerOptions;

    class XorMessagePackMagicOnionSerializer : IMagicOnionSerializer
    {
        readonly MessagePackSerializerOptions serializerOptions;

        public XorMessagePackMagicOnionSerializer(MessagePackSerializerOptions serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }

        public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
        {
            var array = ArrayPool<byte>.Shared.Rent((int)bytes.Length);
            try
            {
                bytes.CopyTo(array);
                for (var i = 0; i < bytes.Length; i++)
                {
                    array[i] ^= MagicNumber;
                }
                return MessagePackSerializer.Deserialize<T>(array.AsMemory(0, (int)bytes.Length), serializerOptions);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public void Serialize<T>(IBufferWriter<byte> writer, in T value)
        {
            var serialized = MessagePackSerializer.Serialize(value, serializerOptions);
            for (var i = 0; i < serialized.Length; i++)
            {
                serialized[i] ^= MagicNumber;
            }
            writer.Write(serialized);
        }
    }

    public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        => new XorMessagePackMagicOnionSerializer(serializerOptions);
}
```
