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

## Example code 2
Introducing the AES256 encryption method.

At first I wanted to handle it by putting the IV in the header.


But I can't find a proper way to parse the header from 'IMagicOnionSerializerProvider'.

So I processed it using the iv prepend method. I created an example because I wanted to share it with you.

IV was created during serialization and decoding.
Place the IV value at the front of the byte array, and place the encrypted data at the back.

```csharp
public class AES256SerializerProvider : IMagicOnionSerializerProvider
{
    private static readonly int IV_LENGTH = 16;
 

    public AES256SerializerProvider()
    {
    }
    
    public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        => new AESSerializer();
    
    class AESSerializer : IMagicOnionSerializer
    {
        readonly MessagePackSerializerOptions serializerOptions;

        public AESSerializer()
        {
            this.serializerOptions = MessagePackSerializer.DefaultOptions;
        }

        public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
        {
            var array = ArrayPool<byte>.Shared.Rent((int)bytes.Length);
            try
            {
                bytes.CopyTo(array);
                int bodySize = array.Length - IV_LENGTH;

                ArraySegment<byte> nonce = new ArraySegment<byte>(array, 0, IV_LENGTH);
                ArraySegment<byte> body = new ArraySegment<byte>(array, IV_LENGTH, bodySize);

                // The AESCipher object is an AES encryption object that uses the .net core native library.
                AESCipher cipher = new AESCipher();
                var packet = cipher.Decrypt(nonce.ToArray(), body.ToArray());
                
                return MessagePackSerializer.Deserialize<T>(packet.AsMemory(0, (int)packet.Length), serializerOptions);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public void Serialize<T>(IBufferWriter<byte> buffer, in T value)
        {
            var planBytes = MessagePackSerializer.Serialize(value, serializerOptions);

            byte[] nonce = new byte[IV_LENGTH];
            RandomNumberGenerator.Fill(nonce);
            
            AESCipher cipher = new AESCipher();
            var encryptData = cipher.Encrypt(nonce, planBytes);

            int totalSize = planBytes.Length + IV_LENGTH;
            
            var span = buffer.GetSpan(totalSize);
            int offset = 0;
            
            nonce.AsSpan().CopyTo(span.Slice(offset)); 
            offset += IV_LENGTH;

            encryptData.AsSpan().CopyTo(span.Slice(offset)); 
            offset += encryptData.Length;
            
            buffer.Advance(offset);
        }
    }
}

```

