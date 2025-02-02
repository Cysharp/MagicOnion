# 메시지의 직렬화와 암호화 커스터마이징

MagicOnion은 기본적으로 통신의 직렬화에 MessagePack을 사용하지만, 직렬화 처리를 커스터마이징하기 위한 확장 포인트도 제공합니다. 커스터마이징은 MessagePack 이외의 시리얼라이저 사용이나 암호화 등이 포함됩니다.

## 커스터마이징 방법

직렬화의 커스터마이징은 `IMagicOnionSerializerProvider` 인터페이스와 그 `Create` 메소드가 반환하는 `IMagicOnionSerializer` 인터페이스를 구현한 시리얼라이저를 구현합니다.

구현한 시리얼라이저 프로바이더를 `MagicOnionSerializerProvider.Default` 프로퍼티에 설정하거나, `MagicOnionClient`, `StreamingHubClient`의 인자로 전달하여 사용할 수 있습니다.

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

## 샘플 코드
다음 코드는 XOR 암호화를 수행하는 간단한 예시입니다.

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
