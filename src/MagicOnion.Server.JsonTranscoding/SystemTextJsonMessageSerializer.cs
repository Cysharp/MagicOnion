using MagicOnion.Serialization;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace MagicOnion.Server.JsonTranscoding;

internal class SystemTextJsonMessageSerializer : IMagicOnionSerializer
{
    readonly JsonSerializerOptions options;
    readonly string[] dynamicArgumentTupleParameterNames;
    readonly bool serializeAsKeyedObject;

    public SystemTextJsonMessageSerializer(JsonSerializerOptions options, string[] dynamicArgumentTupleParameterNames, bool serializeAsKeyedObject = false)
    {
        this.options = new JsonSerializerOptions(options);
        this.options.Converters.Add(NilConverter.Instance);
        this.options.Converters.Add(new DynamicArgumentTupleConverterFactory(dynamicArgumentTupleParameterNames, serializeAsKeyedObject));

        this.dynamicArgumentTupleParameterNames = dynamicArgumentTupleParameterNames;
        this.serializeAsKeyedObject = serializeAsKeyedObject;
    }

    public void Serialize<T>(IBufferWriter<byte> writer, in T value)
    {
        JsonSerializer.Serialize(new Utf8JsonWriter(writer), value, options);
    }

    public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
    {
        var reader = new Utf8JsonReader(bytes);
        return JsonSerializer.Deserialize<T>(ref reader, options)!;
    }

    class NilConverter : JsonConverter<Nil>
    {
        public static NilConverter Instance { get; } = new();

        public override Nil Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Skip();
            return Nil.Default;
        }

        public override void Write(Utf8JsonWriter writer, Nil value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
        }
    }
}
