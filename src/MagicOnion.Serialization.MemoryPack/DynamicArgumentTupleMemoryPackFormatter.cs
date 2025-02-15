﻿// <auto-generated />
#pragma warning disable CS8669
#nullable enable

using System.Runtime.CompilerServices;
using MemoryPack;

namespace MagicOnion.Serialization.MemoryPack;

public static class DynamicArgumentTupleFormatter
{
    static bool isRegistered = false;
    public static void Register()
    {
        if (isRegistered) return;
        isRegistered = true;

        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,>), typeof(DynamicArgumentTupleFormatter<,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,>), typeof(DynamicArgumentTupleFormatter<,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,>), typeof(DynamicArgumentTupleFormatter<,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,,,,,>));
        MemoryPackFormatterProvider.RegisterGenericType(typeof(DynamicArgumentTuple<,,,,,,,,,,,,,,>), typeof(DynamicArgumentTupleFormatter<,,,,,,,,,,,,,,>));
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
        writer.WriteValue(value.Item11);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
            , reader.ReadValue<T11?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
        writer.WriteValue(value.Item11);
        writer.WriteValue(value.Item12);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
            , reader.ReadValue<T11?>()
            , reader.ReadValue<T12?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
        writer.WriteValue(value.Item11);
        writer.WriteValue(value.Item12);
        writer.WriteValue(value.Item13);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
            , reader.ReadValue<T11?>()
            , reader.ReadValue<T12?>()
            , reader.ReadValue<T13?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
        writer.WriteValue(value.Item11);
        writer.WriteValue(value.Item12);
        writer.WriteValue(value.Item13);
        writer.WriteValue(value.Item14);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
            , reader.ReadValue<T11?>()
            , reader.ReadValue<T12?>()
            , reader.ReadValue<T13?>()
            , reader.ReadValue<T14?>()
        );
    }
}

[global::MagicOnion.Serialization.MemoryPack.Preserve]
public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MemoryPackFormatter<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>>
{
    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?> value)
#else
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Item8);
        writer.WriteValue(value.Item9);
        writer.WriteValue(value.Item10);
        writer.WriteValue(value.Item11);
        writer.WriteValue(value.Item12);
        writer.WriteValue(value.Item13);
        writer.WriteValue(value.Item14);
        writer.WriteValue(value.Item15);
    }

    [global::MagicOnion.Serialization.MemoryPack.Preserve]
#if UNITY_2021_3_OR_NEWER
    public override void Deserialize(ref MemoryPackReader reader, ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?> value)
#else
    public override void Deserialize(ref MemoryPackReader reader, scoped ref DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?> value)
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new DynamicArgumentTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, T13?, T14?, T15?>(
            reader.ReadValue<T1?>()
            , reader.ReadValue<T2?>()
            , reader.ReadValue<T3?>()
            , reader.ReadValue<T4?>()
            , reader.ReadValue<T5?>()
            , reader.ReadValue<T6?>()
            , reader.ReadValue<T7?>()
            , reader.ReadValue<T8?>()
            , reader.ReadValue<T9?>()
            , reader.ReadValue<T10?>()
            , reader.ReadValue<T11?>()
            , reader.ReadValue<T12?>()
            , reader.ReadValue<T13?>()
            , reader.ReadValue<T14?>()
            , reader.ReadValue<T15?>()
        );
    }
}


