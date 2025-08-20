# Customizing message serialization and encryption
Introducing the AES256 encryption method.

At first I wanted to handle it by putting the IV in the header.


But I can't find a proper way to parse the header from 'IMagicOnionSerializerProvider'.

So I processed it using the iv prepend method. I created an example because I wanted to share it with you.

IV was created during serialization and decoding.
Place the IV value at the front of the byte array, and place the encrypted data at the back.

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

/// <summary>
/// This is an encryption class using AES.
/// You can change the key value to whatever value you want.
/// However, it must comply with the AES standard.
/// </summary>
public class AESCipher
{
    // aes256 key ( 32byte )
    private static byte[] SecurityKey = Encoding.UTF8.GetBytes("11111111111111111111111111111111");
    
    public byte[] Encrypt(byte[] iv, byte[] packetData)
    {
        if(null == iv)
            throw new NullReferenceException("IV cannot be null");
        
        if(null == packetData)
            throw new NullReferenceException("Packet data cannot be null");
        
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var enc = aes.CreateEncryptor(SecurityKey, iv);
        return enc.TransformFinalBlock(packetData, 0, packetData.Length);
    }
    
    public byte[] Decrypt(byte[]? iv, byte[]? packetData)
    {
        if(null == iv)
            throw new NullReferenceException("IV cannot be null");
        
        if(null == packetData)
            throw new NullReferenceException("Packet data cannot be null");
        
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor(SecurityKey, iv);
        return decryptor.TransformFinalBlock(packetData, 0, packetData.Length);
    }
}

```


## Example code

```csharp
public class AES256SerializerProvider : IMagicOnionSerializerProvider
{
    private static readonly int IvLength = 16;
 

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
                int bodySize = array.Length - IvLength;

                ArraySegment<byte> nonce = new ArraySegment<byte>(array, 0, IvLength);
                ArraySegment<byte> body = new ArraySegment<byte>(array, IvLength, bodySize);

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

            int totalSize = encryptData.Length + IvLength;
            
            var span = buffer.GetSpan(totalSize);
            int offset = 0;
            
            nonce.AsSpan().CopyTo(span.Slice(offset)); 
            offset += IvLength;

            encryptData.AsSpan().CopyTo(span.Slice(offset)); 
            offset += encryptData.Length;
            
            buffer.Advance(offset);
        }
    }
}

```
