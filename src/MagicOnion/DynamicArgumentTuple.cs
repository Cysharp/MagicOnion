
using System;
using MessagePack;
using MessagePack.Formatters;

namespace MagicOnion
{
    // T2 ~ T20

    
    public struct DynamicArgumentTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public DynamicArgumentTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2>>
    {
        readonly T1 default1;
        readonly T2 default2;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2)
        {
            this.default1 = default1;
            this.default2 = default2;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2>(default1, default2);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2>(item1, default2);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2>(item1, item2);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(default1, default2, default3);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(item1, default2, default3);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(item1, item2, default3);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3>(item1, item2, item3);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(default1, default2, default3, default4);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, default2, default3, default4);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, default3, default4);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, item3, default4);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(default1, default2, default3, default4, default5);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, default2, default3, default4, default5);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, default3, default4, default5);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, default4, default5);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, default5);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(default1, default2, default3, default4, default5, default6);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, default2, default3, default4, default5, default6);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, default3, default4, default5, default6);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, default4, default5, default6);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, default5, default6);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, default6);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(default1, default2, default3, default4, default5, default6, default7);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, default2, default3, default4, default5, default6, default7);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, default3, default4, default5, default6, default7);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, default4, default5, default6, default7);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, default5, default6, default7);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, default6, default7);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, default7);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(default1, default2, default3, default4, default5, default6, default7, default8);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, default2, default3, default4, default5, default6, default7, default8);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, default3, default4, default5, default6, default7, default8);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, default4, default5, default6, default7, default8);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, default5, default6, default7, default8);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, default6, default7, default8);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, default7, default8);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, default8);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(default1, default2, default3, default4, default5, default6, default7, default8, default9);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, default2, default3, default4, default5, default6, default7, default8, default9);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, default3, default4, default5, default6, default7, default8, default9);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, default4, default5, default6, default7, default8, default9);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, default5, default6, default7, default8, default9);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, default6, default7, default8, default9);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, default7, default8, default9);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, default8, default9);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, default9);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, item9);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;
        public readonly T16 Item16;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
            Item16 = item16;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;
        readonly T16 default16;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15, T16 default16)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
            this.default16 = default16;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T16>().Serialize(ref bytes, offset, value.Item16, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16);

            var item16 = formatterResolver.GetFormatterWithVerify<T16>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;
        public readonly T16 Item16;
        public readonly T17 Item17;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16, T17 item17)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
            Item16 = item16;
            Item17 = item17;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;
        readonly T16 default16;
        readonly T17 default17;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15, T16 default16, T17 default17)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
            this.default16 = default16;
            this.default17 = default17;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T16>().Serialize(ref bytes, offset, value.Item16, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T17>().Serialize(ref bytes, offset, value.Item17, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17);

            var item16 = formatterResolver.GetFormatterWithVerify<T16>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17);

            var item17 = formatterResolver.GetFormatterWithVerify<T17>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;
        public readonly T16 Item16;
        public readonly T17 Item17;
        public readonly T18 Item18;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16, T17 item17, T18 item18)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
            Item16 = item16;
            Item17 = item17;
            Item18 = item18;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;
        readonly T16 default16;
        readonly T17 default17;
        readonly T18 default18;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15, T16 default16, T17 default17, T18 default18)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
            this.default16 = default16;
            this.default17 = default17;
            this.default18 = default18;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T16>().Serialize(ref bytes, offset, value.Item16, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T17>().Serialize(ref bytes, offset, value.Item17, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T18>().Serialize(ref bytes, offset, value.Item18, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18);

            var item16 = formatterResolver.GetFormatterWithVerify<T16>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18);

            var item17 = formatterResolver.GetFormatterWithVerify<T17>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18);

            var item18 = formatterResolver.GetFormatterWithVerify<T18>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;
        public readonly T16 Item16;
        public readonly T17 Item17;
        public readonly T18 Item18;
        public readonly T19 Item19;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16, T17 item17, T18 item18, T19 item19)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
            Item16 = item16;
            Item17 = item17;
            Item18 = item18;
            Item19 = item19;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;
        readonly T16 default16;
        readonly T17 default17;
        readonly T18 default18;
        readonly T19 default19;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15, T16 default16, T17 default17, T18 default18, T19 default19)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
            this.default16 = default16;
            this.default17 = default17;
            this.default18 = default18;
            this.default19 = default19;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T16>().Serialize(ref bytes, offset, value.Item16, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T17>().Serialize(ref bytes, offset, value.Item17, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T18>().Serialize(ref bytes, offset, value.Item18, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T19>().Serialize(ref bytes, offset, value.Item19, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18, default19);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18, default19);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18, default19);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18, default19);

            var item16 = formatterResolver.GetFormatterWithVerify<T16>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18, default19);

            var item17 = formatterResolver.GetFormatterWithVerify<T17>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18, default19);

            var item18 = formatterResolver.GetFormatterWithVerify<T18>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, default19);

            var item19 = formatterResolver.GetFormatterWithVerify<T19>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19);
        }
    }
    
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;
        public readonly T6 Item6;
        public readonly T7 Item7;
        public readonly T8 Item8;
        public readonly T9 Item9;
        public readonly T10 Item10;
        public readonly T11 Item11;
        public readonly T12 Item12;
        public readonly T13 Item13;
        public readonly T14 Item14;
        public readonly T15 Item15;
        public readonly T16 Item16;
        public readonly T17 Item17;
        public readonly T18 Item18;
        public readonly T19 Item19;
        public readonly T20 Item20;

        public DynamicArgumentTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16, T17 item17, T18 item18, T19 item19, T20 item20)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
            Item10 = item10;
            Item11 = item11;
            Item12 = item12;
            Item13 = item13;
            Item14 = item14;
            Item15 = item15;
            Item16 = item16;
            Item17 = item17;
            Item18 = item18;
            Item19 = item19;
            Item20 = item20;
        }
    }

    public class DynamicArgumentTupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> : IMessagePackFormatter<DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>>
    {
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;
        readonly T8 default8;
        readonly T9 default9;
        readonly T10 default10;
        readonly T11 default11;
        readonly T12 default12;
        readonly T13 default13;
        readonly T14 default14;
        readonly T15 default15;
        readonly T16 default16;
        readonly T17 default17;
        readonly T18 default18;
        readonly T19 default19;
        readonly T20 default20;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7, T8 default8, T9 default9, T10 default10, T11 default11, T12 default12, T13 default13, T14 default14, T15 default15, T16 default16, T17 default17, T18 default18, T19 default19, T20 default20)
        {
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.default10 = default10;
            this.default11 = default11;
            this.default12 = default12;
            this.default13 = default13;
            this.default14 = default14;
            this.default15 = default15;
            this.default16 = default16;
            this.default17 = default17;
            this.default18 = default18;
            this.default19 = default19;
            this.default20 = default20;
        }

        public int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref bytes, offset, value.Item2, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref bytes, offset, value.Item3, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref bytes, offset, value.Item4, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T5>().Serialize(ref bytes, offset, value.Item5, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T6>().Serialize(ref bytes, offset, value.Item6, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T7>().Serialize(ref bytes, offset, value.Item7, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T8>().Serialize(ref bytes, offset, value.Item8, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T9>().Serialize(ref bytes, offset, value.Item9, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T10>().Serialize(ref bytes, offset, value.Item10, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T11>().Serialize(ref bytes, offset, value.Item11, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T12>().Serialize(ref bytes, offset, value.Item12, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T13>().Serialize(ref bytes, offset, value.Item13, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T14>().Serialize(ref bytes, offset, value.Item14, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T15>().Serialize(ref bytes, offset, value.Item15, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T16>().Serialize(ref bytes, offset, value.Item16, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T17>().Serialize(ref bytes, offset, value.Item17, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T18>().Serialize(ref bytes, offset, value.Item18, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T19>().Serialize(ref bytes, offset, value.Item19, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<T20>().Serialize(ref bytes, offset, value.Item20, formatterResolver);
            return offset - startOffset;
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item5 = formatterResolver.GetFormatterWithVerify<T5>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item6 = formatterResolver.GetFormatterWithVerify<T6>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item7 = formatterResolver.GetFormatterWithVerify<T7>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item8 = formatterResolver.GetFormatterWithVerify<T8>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item9 = formatterResolver.GetFormatterWithVerify<T9>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item10 = formatterResolver.GetFormatterWithVerify<T10>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item11 = formatterResolver.GetFormatterWithVerify<T11>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item12 = formatterResolver.GetFormatterWithVerify<T12>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item13 = formatterResolver.GetFormatterWithVerify<T13>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18, default19, default20);

            var item14 = formatterResolver.GetFormatterWithVerify<T14>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18, default19, default20);

            var item15 = formatterResolver.GetFormatterWithVerify<T15>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18, default19, default20);

            var item16 = formatterResolver.GetFormatterWithVerify<T16>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18, default19, default20);

            var item17 = formatterResolver.GetFormatterWithVerify<T17>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18, default19, default20);

            var item18 = formatterResolver.GetFormatterWithVerify<T18>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, default19, default20);

            var item19 = formatterResolver.GetFormatterWithVerify<T19>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, default20);

            var item20 = formatterResolver.GetFormatterWithVerify<T20>().Deserialize(bytes, offset, formatterResolver, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, item20);
        }
    }
}