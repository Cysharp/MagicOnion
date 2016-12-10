
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace MagicOnion
{
    internal static class FormatterLengthHelper
    {
        internal static int? GetLength(params IFormatter[] formatters)
        {
            int? sum = 0;
            foreach (var item in formatters)
            {
                var len = item.GetLength();
                if (len == null) return null;
                sum += len;
            }
            return sum;
        }
    }

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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly T1 default1;
        readonly T2 default2;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2>(default1, default2);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2>(item1, default2);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(default1, default2, default3);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(item1, default2, default3);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3>(item1, item2, default3);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(default1, default2, default3, default4);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, default2, default3, default4);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, default3, default4);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, item3, default4);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(default1, default2, default3, default4, default5);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, default2, default3, default4, default5);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, default3, default4, default5);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, default4, default5);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, default5);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(default1, default2, default3, default4, default5, default6);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, default2, default3, default4, default5, default6);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, default3, default4, default5, default6);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, default4, default5, default6);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, default5, default6);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, default6);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly T1 default1;
        readonly T2 default2;
        readonly T3 default3;
        readonly T4 default4;
        readonly T5 default5;
        readonly T6 default6;
        readonly T7 default7;

        public DynamicArgumentTupleFormatter(T1 default1, T2 default2, T3 default3, T4 default4, T5 default5, T6 default6, T7 default7)
        {
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(default1, default2, default3, default4, default5, default6, default7);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, default2, default3, default4, default5, default6, default7);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, default3, default4, default5, default6, default7);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, default4, default5, default6, default7);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, default5, default6, default7);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, default6, default7);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, default7);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(default1, default2, default3, default4, default5, default6, default7, default8);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, default2, default3, default4, default5, default6, default7, default8);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, default3, default4, default5, default6, default7, default8);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, default4, default5, default6, default7, default8);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, default5, default6, default7, default8);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, default6, default7, default8);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, default7, default8);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, default8);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.default1 = default1;
            this.default2 = default2;
            this.default3 = default3;
            this.default4 = default4;
            this.default5 = default5;
            this.default6 = default6;
            this.default7 = default7;
            this.default8 = default8;
            this.default9 = default9;
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(default1, default2, default3, default4, default5, default6, default7, default8, default9);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, default2, default3, default4, default5, default6, default7, default8, default9);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, default3, default4, default5, default6, default7, default8, default9);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, default4, default5, default6, default7, default8, default9);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, default5, default6, default7, default8, default9);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, default6, default7, default8, default9);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, default7, default8, default9);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, default8, default9);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, default9);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
        readonly Formatter<TTypeResolver, T16> formatter16;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
            this.formatter16 = Formatter<TTypeResolver, T16>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15, formatter16);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker && formatter16.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            offset += this.formatter16.Serialize(ref bytes, offset, value.Item16);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16);

            var item16 = this.formatter16.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
        readonly Formatter<TTypeResolver, T16> formatter16;
        readonly Formatter<TTypeResolver, T17> formatter17;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
            this.formatter16 = Formatter<TTypeResolver, T16>.Default;
            this.formatter17 = Formatter<TTypeResolver, T17>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15, formatter16, formatter17);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker && formatter16.NoUseDirtyTracker && formatter17.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            offset += this.formatter16.Serialize(ref bytes, offset, value.Item16);
            offset += this.formatter17.Serialize(ref bytes, offset, value.Item17);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17);

            var item16 = this.formatter16.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17);

            var item17 = this.formatter17.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
        readonly Formatter<TTypeResolver, T16> formatter16;
        readonly Formatter<TTypeResolver, T17> formatter17;
        readonly Formatter<TTypeResolver, T18> formatter18;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
            this.formatter16 = Formatter<TTypeResolver, T16>.Default;
            this.formatter17 = Formatter<TTypeResolver, T17>.Default;
            this.formatter18 = Formatter<TTypeResolver, T18>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15, formatter16, formatter17, formatter18);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker && formatter16.NoUseDirtyTracker && formatter17.NoUseDirtyTracker && formatter18.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            offset += this.formatter16.Serialize(ref bytes, offset, value.Item16);
            offset += this.formatter17.Serialize(ref bytes, offset, value.Item17);
            offset += this.formatter18.Serialize(ref bytes, offset, value.Item18);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18);

            var item16 = this.formatter16.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18);

            var item17 = this.formatter17.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18);

            var item18 = this.formatter18.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
        readonly Formatter<TTypeResolver, T16> formatter16;
        readonly Formatter<TTypeResolver, T17> formatter17;
        readonly Formatter<TTypeResolver, T18> formatter18;
        readonly Formatter<TTypeResolver, T19> formatter19;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
            this.formatter16 = Formatter<TTypeResolver, T16>.Default;
            this.formatter17 = Formatter<TTypeResolver, T17>.Default;
            this.formatter18 = Formatter<TTypeResolver, T18>.Default;
            this.formatter19 = Formatter<TTypeResolver, T19>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15, formatter16, formatter17, formatter18, formatter19);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker && formatter16.NoUseDirtyTracker && formatter17.NoUseDirtyTracker && formatter18.NoUseDirtyTracker && formatter19.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            offset += this.formatter16.Serialize(ref bytes, offset, value.Item16);
            offset += this.formatter17.Serialize(ref bytes, offset, value.Item17);
            offset += this.formatter18.Serialize(ref bytes, offset, value.Item18);
            offset += this.formatter19.Serialize(ref bytes, offset, value.Item19);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18, default19);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18, default19);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18, default19);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18, default19);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18, default19);

            var item16 = this.formatter16.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18, default19);

            var item17 = this.formatter17.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18, default19);

            var item18 = this.formatter18.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, default19);

            var item19 = this.formatter19.Deserialize(ref bytes, offset, tracker, out size);
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

    public class DynamicArgumentTupleFormatter<TTypeResolver, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> : Formatter<TTypeResolver, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly int? length;
        readonly bool noUseDirtyTracker;
        readonly Formatter<TTypeResolver, T1> formatter1;
        readonly Formatter<TTypeResolver, T2> formatter2;
        readonly Formatter<TTypeResolver, T3> formatter3;
        readonly Formatter<TTypeResolver, T4> formatter4;
        readonly Formatter<TTypeResolver, T5> formatter5;
        readonly Formatter<TTypeResolver, T6> formatter6;
        readonly Formatter<TTypeResolver, T7> formatter7;
        readonly Formatter<TTypeResolver, T8> formatter8;
        readonly Formatter<TTypeResolver, T9> formatter9;
        readonly Formatter<TTypeResolver, T10> formatter10;
        readonly Formatter<TTypeResolver, T11> formatter11;
        readonly Formatter<TTypeResolver, T12> formatter12;
        readonly Formatter<TTypeResolver, T13> formatter13;
        readonly Formatter<TTypeResolver, T14> formatter14;
        readonly Formatter<TTypeResolver, T15> formatter15;
        readonly Formatter<TTypeResolver, T16> formatter16;
        readonly Formatter<TTypeResolver, T17> formatter17;
        readonly Formatter<TTypeResolver, T18> formatter18;
        readonly Formatter<TTypeResolver, T19> formatter19;
        readonly Formatter<TTypeResolver, T20> formatter20;
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
            this.formatter1 = Formatter<TTypeResolver, T1>.Default;
            this.formatter2 = Formatter<TTypeResolver, T2>.Default;
            this.formatter3 = Formatter<TTypeResolver, T3>.Default;
            this.formatter4 = Formatter<TTypeResolver, T4>.Default;
            this.formatter5 = Formatter<TTypeResolver, T5>.Default;
            this.formatter6 = Formatter<TTypeResolver, T6>.Default;
            this.formatter7 = Formatter<TTypeResolver, T7>.Default;
            this.formatter8 = Formatter<TTypeResolver, T8>.Default;
            this.formatter9 = Formatter<TTypeResolver, T9>.Default;
            this.formatter10 = Formatter<TTypeResolver, T10>.Default;
            this.formatter11 = Formatter<TTypeResolver, T11>.Default;
            this.formatter12 = Formatter<TTypeResolver, T12>.Default;
            this.formatter13 = Formatter<TTypeResolver, T13>.Default;
            this.formatter14 = Formatter<TTypeResolver, T14>.Default;
            this.formatter15 = Formatter<TTypeResolver, T15>.Default;
            this.formatter16 = Formatter<TTypeResolver, T16>.Default;
            this.formatter17 = Formatter<TTypeResolver, T17>.Default;
            this.formatter18 = Formatter<TTypeResolver, T18>.Default;
            this.formatter19 = Formatter<TTypeResolver, T19>.Default;
            this.formatter20 = Formatter<TTypeResolver, T20>.Default;
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
            this.length = FormatterLengthHelper.GetLength(formatter1, formatter2, formatter3, formatter4, formatter5, formatter6, formatter7, formatter8, formatter9, formatter10, formatter11, formatter12, formatter13, formatter14, formatter15, formatter16, formatter17, formatter18, formatter19, formatter20);
            this.noUseDirtyTracker = formatter1.NoUseDirtyTracker && formatter2.NoUseDirtyTracker && formatter3.NoUseDirtyTracker && formatter4.NoUseDirtyTracker && formatter5.NoUseDirtyTracker && formatter6.NoUseDirtyTracker && formatter7.NoUseDirtyTracker && formatter8.NoUseDirtyTracker && formatter9.NoUseDirtyTracker && formatter10.NoUseDirtyTracker && formatter11.NoUseDirtyTracker && formatter12.NoUseDirtyTracker && formatter13.NoUseDirtyTracker && formatter14.NoUseDirtyTracker && formatter15.NoUseDirtyTracker && formatter16.NoUseDirtyTracker && formatter17.NoUseDirtyTracker && formatter18.NoUseDirtyTracker && formatter19.NoUseDirtyTracker && formatter20.NoUseDirtyTracker;
        }

        public override bool NoUseDirtyTracker
        {
            get
            {
                return noUseDirtyTracker;
            }
        }

        public override int? GetLength()
        {
            return length;
        }

        public override int Serialize(ref byte[] bytes, int offset, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> value)
        {
            if (length != null && bytes == null)
            {
                bytes = new byte[length.Value];
            }
            var startOffset = offset;
            offset += this.formatter1.Serialize(ref bytes, offset, value.Item1);
            offset += this.formatter2.Serialize(ref bytes, offset, value.Item2);
            offset += this.formatter3.Serialize(ref bytes, offset, value.Item3);
            offset += this.formatter4.Serialize(ref bytes, offset, value.Item4);
            offset += this.formatter5.Serialize(ref bytes, offset, value.Item5);
            offset += this.formatter6.Serialize(ref bytes, offset, value.Item6);
            offset += this.formatter7.Serialize(ref bytes, offset, value.Item7);
            offset += this.formatter8.Serialize(ref bytes, offset, value.Item8);
            offset += this.formatter9.Serialize(ref bytes, offset, value.Item9);
            offset += this.formatter10.Serialize(ref bytes, offset, value.Item10);
            offset += this.formatter11.Serialize(ref bytes, offset, value.Item11);
            offset += this.formatter12.Serialize(ref bytes, offset, value.Item12);
            offset += this.formatter13.Serialize(ref bytes, offset, value.Item13);
            offset += this.formatter14.Serialize(ref bytes, offset, value.Item14);
            offset += this.formatter15.Serialize(ref bytes, offset, value.Item15);
            offset += this.formatter16.Serialize(ref bytes, offset, value.Item16);
            offset += this.formatter17.Serialize(ref bytes, offset, value.Item17);
            offset += this.formatter18.Serialize(ref bytes, offset, value.Item18);
            offset += this.formatter19.Serialize(ref bytes, offset, value.Item19);
            offset += this.formatter20.Serialize(ref bytes, offset, value.Item20);
            return offset - startOffset;
        }

        public override DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;

            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(default1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item1 = this.formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, default2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item2 = this.formatter2.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, default3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item3 = this.formatter3.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, default4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item4 = this.formatter4.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, default5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item5 = this.formatter5.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, default6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item6 = this.formatter6.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, default7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item7 = this.formatter7.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, default8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item8 = this.formatter8.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, default9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item9 = this.formatter9.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, default10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item10 = this.formatter10.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, default11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item11 = this.formatter11.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, default12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item12 = this.formatter12.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, default13, default14, default15, default16, default17, default18, default19, default20);

            var item13 = this.formatter13.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, default14, default15, default16, default17, default18, default19, default20);

            var item14 = this.formatter14.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, default15, default16, default17, default18, default19, default20);

            var item15 = this.formatter15.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, default16, default17, default18, default19, default20);

            var item16 = this.formatter16.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, default17, default18, default19, default20);

            var item17 = this.formatter17.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, default18, default19, default20);

            var item18 = this.formatter18.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, default19, default20);

            var item19 = this.formatter19.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            if (bytes.Length == byteSize) return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, default20);

            var item20 = this.formatter20.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, item20);
        }
    }
}