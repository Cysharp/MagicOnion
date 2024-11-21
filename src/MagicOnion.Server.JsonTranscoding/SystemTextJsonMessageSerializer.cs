using MagicOnion.Serialization;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace MagicOnion.Server.JsonTranscoding;

internal class SystemTextJsonMessageSerializer : IMagicOnionSerializer
{
    readonly JsonSerializerOptions options;

    public SystemTextJsonMessageSerializer(JsonSerializerOptions options)
    {
        this.options = new JsonSerializerOptions(options);
        this.options.Converters.Add(NilConverter.Instance);
        this.options.Converters.Add(DynamicArgumentTupleConverterFactory.Instance);
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

    class DynamicArgumentTupleConverterFactory : JsonConverterFactory
    {
        public static DynamicArgumentTupleConverterFactory Instance { get; } = new();

        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType) return false;
            var openType = typeToConvert.GetGenericTypeDefinition();

            return openType == typeof(DynamicArgumentTuple<,>) ||
                   openType == typeof(DynamicArgumentTuple<,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,,,,,>) ||
                   openType == typeof(DynamicArgumentTuple<,,,,,,,,,,,,,,>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var openType = typeToConvert.GetGenericTypeDefinition()!;
            var genericArguments = typeToConvert.GetGenericArguments();

            var converterType = genericArguments.Length switch
            {
                2 => typeof(DynamicArgumentTupleConverter<,>).MakeGenericType(genericArguments),
                3 => typeof(DynamicArgumentTupleConverter<,,>).MakeGenericType(genericArguments),
                4 => typeof(DynamicArgumentTupleConverter<,,,>).MakeGenericType(genericArguments),
                5 => typeof(DynamicArgumentTupleConverter<,,,,>).MakeGenericType(genericArguments),
                6 => typeof(DynamicArgumentTupleConverter<,,,,,>).MakeGenericType(genericArguments),
                7 => typeof(DynamicArgumentTupleConverter<,,,,,,>).MakeGenericType(genericArguments),
                8 => typeof(DynamicArgumentTupleConverter<,,,,,,,>).MakeGenericType(genericArguments),
                9 => typeof(DynamicArgumentTupleConverter<,,,,,,,,>).MakeGenericType(genericArguments),
                10 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,>).MakeGenericType(genericArguments),
                11 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,,>).MakeGenericType(genericArguments),
                12 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,,,>).MakeGenericType(genericArguments),
                13 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,,,,>).MakeGenericType(genericArguments),
                14 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,,,,,>).MakeGenericType(genericArguments),
                15 => typeof(DynamicArgumentTupleConverter<,,,,,,,,,,,,,,>).MakeGenericType(genericArguments),
                _ => throw new NotSupportedException()
            };

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
        class DynamicArgumentTupleConverter<T1, T2> : JsonConverter<DynamicArgumentTuple<T1, T2>>
        {
            public override DynamicArgumentTuple<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2>(item1!, item2!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3> : JsonConverter<DynamicArgumentTuple<T1, T2, T3>>
        {
            public override DynamicArgumentTuple<T1, T2, T3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3>(item1!, item2!, item3!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4>(item1!, item2!, item3!, item4!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1!, item2!, item3!, item4!, item5!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1!, item2!, item3!, item4!, item5!, item6!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1!, item2!, item3!, item4!, item5!, item6!, item7!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                var item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                var item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                var item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                JsonSerializer.Serialize(writer, value.Item11, options);
                JsonSerializer.Serialize(writer, value.Item12, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                var item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                var item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                var item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                JsonSerializer.Serialize(writer, value.Item11, options);
                JsonSerializer.Serialize(writer, value.Item12, options);
                JsonSerializer.Serialize(writer, value.Item13, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                var item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                var item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                var item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
                var item14 = JsonSerializer.Deserialize<T14>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!, item14!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                JsonSerializer.Serialize(writer, value.Item11, options);
                JsonSerializer.Serialize(writer, value.Item12, options);
                JsonSerializer.Serialize(writer, value.Item13, options);
                JsonSerializer.Serialize(writer, value.Item14, options);
                writer.WriteEndArray();
            }
        }
        class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
        {
            public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                var item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                var item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                var item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                var item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                var item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                var item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                var item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                var item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                var item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                var item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                var item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                var item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
                var item14 = JsonSerializer.Deserialize<T14>(ref reader, options);
                reader.Read();
                var item15 = JsonSerializer.Deserialize<T15>(ref reader, options);
                reader.Read();
                return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!, item14!, item15!);
            }

            public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                JsonSerializer.Serialize(writer, value.Item5, options);
                JsonSerializer.Serialize(writer, value.Item6, options);
                JsonSerializer.Serialize(writer, value.Item7, options);
                JsonSerializer.Serialize(writer, value.Item8, options);
                JsonSerializer.Serialize(writer, value.Item9, options);
                JsonSerializer.Serialize(writer, value.Item10, options);
                JsonSerializer.Serialize(writer, value.Item11, options);
                JsonSerializer.Serialize(writer, value.Item12, options);
                JsonSerializer.Serialize(writer, value.Item13, options);
                JsonSerializer.Serialize(writer, value.Item14, options);
                JsonSerializer.Serialize(writer, value.Item15, options);
                writer.WriteEndArray();
            }
        }
    }
}
