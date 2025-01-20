# メッセージのシリアライズと暗号化のカスタマイズ

MagicOnion はデフォルトで通信のシリアライズに MessagePack を使用しますが、シリアライズ処理をカスタマイズするための拡張ポイントも提供しています。カスタマイズは MessagePack 以外のシリアライザーの使用や暗号化などが含まれます。

## カスタマイズ方法

シリアライズのカスタマイズは `IMagicOnionSerializerProvider` インターフェースとその `Create` メソッドが返す `IMagicOnionSerializer` インターフェースを実装したシリアライザーを実装します。

実装したシリアライザープロバイダーを `MagicOnionSerializerProvider.Default` プロパティーにセットするか、`MagicOnionClient`, `StramingHubClient` の引数に渡すことで使用できます。

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

## サンプルコード
以下のコードは XOR 暗号化を行う簡単な例です。

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
