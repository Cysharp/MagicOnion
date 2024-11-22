using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicOnion.Server.JsonTranscoding;

internal class DynamicArgumentTupleConverterFactory(string[]? parameterNames, bool serializeAsKeyedObject) : JsonConverterFactory
{
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

        return (JsonConverter)Activator.CreateInstance(converterType, parameterNames, serializeAsKeyedObject)!;
    }

    class DynamicArgumentTupleConverter<T1, T2>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2>>
    {
        public override DynamicArgumentTuple<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2>(item1!, item2!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WriteEndArray();
            }
        }
    }
    class DynamicArgumentTupleConverter<T1, T2, T3>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3>>
    {
        public override DynamicArgumentTuple<T1, T2, T3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3>(item1!, item2!, item3!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WriteEndArray();
            }
        }
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4>(item1!, item2!, item3!, item4!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, value.Item1, options);
                JsonSerializer.Serialize(writer, value.Item2, options);
                JsonSerializer.Serialize(writer, value.Item3, options);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WriteEndArray();
            }
        }
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1!, item2!, item3!, item4!, item5!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1!, item2!, item3!, item4!, item5!, item6!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1!, item2!, item3!, item4!, item5!, item6!, item7!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            var item11 = default(T11);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                            case 10: item11 = JsonSerializer.Deserialize<T11>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WritePropertyName(parameterNames[10]);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            var item11 = default(T11);
            var item12 = default(T12);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                            case 10: item11 = JsonSerializer.Deserialize<T11>(ref reader, options); break;
                            case 11: item12 = JsonSerializer.Deserialize<T12>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WritePropertyName(parameterNames[10]);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WritePropertyName(parameterNames[11]);
                JsonSerializer.Serialize(writer, value.Item12, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            var item11 = default(T11);
            var item12 = default(T12);
            var item13 = default(T13);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                            case 10: item11 = JsonSerializer.Deserialize<T11>(ref reader, options); break;
                            case 11: item12 = JsonSerializer.Deserialize<T12>(ref reader, options); break;
                            case 12: item13 = JsonSerializer.Deserialize<T13>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WritePropertyName(parameterNames[10]);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WritePropertyName(parameterNames[11]);
                JsonSerializer.Serialize(writer, value.Item12, options);
                writer.WritePropertyName(parameterNames[12]);
                JsonSerializer.Serialize(writer, value.Item13, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            var item11 = default(T11);
            var item12 = default(T12);
            var item13 = default(T13);
            var item14 = default(T14);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
                item14 = JsonSerializer.Deserialize<T14>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                            case 10: item11 = JsonSerializer.Deserialize<T11>(ref reader, options); break;
                            case 11: item12 = JsonSerializer.Deserialize<T12>(ref reader, options); break;
                            case 12: item13 = JsonSerializer.Deserialize<T13>(ref reader, options); break;
                            case 13: item14 = JsonSerializer.Deserialize<T14>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!, item14!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WritePropertyName(parameterNames[10]);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WritePropertyName(parameterNames[11]);
                JsonSerializer.Serialize(writer, value.Item12, options);
                writer.WritePropertyName(parameterNames[12]);
                JsonSerializer.Serialize(writer, value.Item13, options);
                writer.WritePropertyName(parameterNames[13]);
                JsonSerializer.Serialize(writer, value.Item14, options);
                writer.WriteEndObject();
            }
            else
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
    }
    class DynamicArgumentTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string[] parameterNames, bool serializeAsKeyedObject) : JsonConverter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
    {
        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item1 = default(T1);
            var item2 = default(T2);
            var item3 = default(T3);
            var item4 = default(T4);
            var item5 = default(T5);
            var item6 = default(T6);
            var item7 = default(T7);
            var item8 = default(T8);
            var item9 = default(T9);
            var item10 = default(T10);
            var item11 = default(T11);
            var item12 = default(T12);
            var item13 = default(T13);
            var item14 = default(T14);
            var item15 = default(T15);
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                item1 = JsonSerializer.Deserialize<T1>(ref reader, options);
                reader.Read();
                item2 = JsonSerializer.Deserialize<T2>(ref reader, options);
                reader.Read();
                item3 = JsonSerializer.Deserialize<T3>(ref reader, options);
                reader.Read();
                item4 = JsonSerializer.Deserialize<T4>(ref reader, options);
                reader.Read();
                item5 = JsonSerializer.Deserialize<T5>(ref reader, options);
                reader.Read();
                item6 = JsonSerializer.Deserialize<T6>(ref reader, options);
                reader.Read();
                item7 = JsonSerializer.Deserialize<T7>(ref reader, options);
                reader.Read();
                item8 = JsonSerializer.Deserialize<T8>(ref reader, options);
                reader.Read();
                item9 = JsonSerializer.Deserialize<T9>(ref reader, options);
                reader.Read();
                item10 = JsonSerializer.Deserialize<T10>(ref reader, options);
                reader.Read();
                item11 = JsonSerializer.Deserialize<T11>(ref reader, options);
                reader.Read();
                item12 = JsonSerializer.Deserialize<T12>(ref reader, options);
                reader.Read();
                item13 = JsonSerializer.Deserialize<T13>(ref reader, options);
                reader.Read();
                item14 = JsonSerializer.Deserialize<T14>(ref reader, options);
                reader.Read();
                item15 = JsonSerializer.Deserialize<T15>(ref reader, options);
                reader.Read();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName is not null && Array.IndexOf(parameterNames, propName) is { } index and > -1)
                    {
                        reader.Read();
                        switch (index)
                        {
                            case 0: item1 = JsonSerializer.Deserialize<T1>(ref reader, options); break;
                            case 1: item2 = JsonSerializer.Deserialize<T2>(ref reader, options); break;
                            case 2: item3 = JsonSerializer.Deserialize<T3>(ref reader, options); break;
                            case 3: item4 = JsonSerializer.Deserialize<T4>(ref reader, options); break;
                            case 4: item5 = JsonSerializer.Deserialize<T5>(ref reader, options); break;
                            case 5: item6 = JsonSerializer.Deserialize<T6>(ref reader, options); break;
                            case 6: item7 = JsonSerializer.Deserialize<T7>(ref reader, options); break;
                            case 7: item8 = JsonSerializer.Deserialize<T8>(ref reader, options); break;
                            case 8: item9 = JsonSerializer.Deserialize<T9>(ref reader, options); break;
                            case 9: item10 = JsonSerializer.Deserialize<T10>(ref reader, options); break;
                            case 10: item11 = JsonSerializer.Deserialize<T11>(ref reader, options); break;
                            case 11: item12 = JsonSerializer.Deserialize<T12>(ref reader, options); break;
                            case 12: item13 = JsonSerializer.Deserialize<T13>(ref reader, options); break;
                            case 13: item14 = JsonSerializer.Deserialize<T14>(ref reader, options); break;
                            case 14: item15 = JsonSerializer.Deserialize<T15>(ref reader, options); break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("DynamicArgumentTuple must be serialized as Array or Object.");
            }
            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1!, item2!, item3!, item4!, item5!, item6!, item7!, item8!, item9!, item10!, item11!, item12!, item13!, item14!, item15!);
        }

        public override void Write(Utf8JsonWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> value, JsonSerializerOptions options)
        {
            if (serializeAsKeyedObject)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(parameterNames[0]);
                JsonSerializer.Serialize(writer, value.Item1, options);
                writer.WritePropertyName(parameterNames[1]);
                JsonSerializer.Serialize(writer, value.Item2, options);
                writer.WritePropertyName(parameterNames[2]);
                JsonSerializer.Serialize(writer, value.Item3, options);
                writer.WritePropertyName(parameterNames[3]);
                JsonSerializer.Serialize(writer, value.Item4, options);
                writer.WritePropertyName(parameterNames[4]);
                JsonSerializer.Serialize(writer, value.Item5, options);
                writer.WritePropertyName(parameterNames[5]);
                JsonSerializer.Serialize(writer, value.Item6, options);
                writer.WritePropertyName(parameterNames[6]);
                JsonSerializer.Serialize(writer, value.Item7, options);
                writer.WritePropertyName(parameterNames[7]);
                JsonSerializer.Serialize(writer, value.Item8, options);
                writer.WritePropertyName(parameterNames[8]);
                JsonSerializer.Serialize(writer, value.Item9, options);
                writer.WritePropertyName(parameterNames[9]);
                JsonSerializer.Serialize(writer, value.Item10, options);
                writer.WritePropertyName(parameterNames[10]);
                JsonSerializer.Serialize(writer, value.Item11, options);
                writer.WritePropertyName(parameterNames[11]);
                JsonSerializer.Serialize(writer, value.Item12, options);
                writer.WritePropertyName(parameterNames[12]);
                JsonSerializer.Serialize(writer, value.Item13, options);
                writer.WritePropertyName(parameterNames[13]);
                JsonSerializer.Serialize(writer, value.Item14, options);
                writer.WritePropertyName(parameterNames[14]);
                JsonSerializer.Serialize(writer, value.Item15, options);
                writer.WriteEndObject();
            }
            else
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
