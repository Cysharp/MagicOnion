
using System;
using MessagePack;
using MessagePack.Formatters;

namespace MagicOnion
{
    // T2 ~ T20

    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
        }

        public DynamicArgumentTuple<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2>(item1, item2);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
        }

        public DynamicArgumentTuple<T1, T2, T3> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3>(item1, item2, item3);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(5);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(6);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(7);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(8);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(9);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(item1, item2, item3, item4, item5, item6, item7, item8, item9);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(10);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(11);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(12);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(13);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(14);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(15);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;
        [Key(15)]
        public readonly T16 Item16;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(16);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
            resolver.GetFormatterWithVerify<T16>().Serialize(ref writer, value.Item16, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;
            var item16 = default16;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    case 15:
                        item16 = resolver.GetFormatterWithVerify<T16>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;
        [Key(15)]
        public readonly T16 Item16;
        [Key(16)]
        public readonly T17 Item17;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(17);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
            resolver.GetFormatterWithVerify<T16>().Serialize(ref writer, value.Item16, options);
            resolver.GetFormatterWithVerify<T17>().Serialize(ref writer, value.Item17, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;
            var item16 = default16;
            var item17 = default17;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    case 15:
                        item16 = resolver.GetFormatterWithVerify<T16>().Deserialize(ref reader, options);
                        break;
                    case 16:
                        item17 = resolver.GetFormatterWithVerify<T17>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;
        [Key(15)]
        public readonly T16 Item16;
        [Key(16)]
        public readonly T17 Item17;
        [Key(17)]
        public readonly T18 Item18;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(18);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
            resolver.GetFormatterWithVerify<T16>().Serialize(ref writer, value.Item16, options);
            resolver.GetFormatterWithVerify<T17>().Serialize(ref writer, value.Item17, options);
            resolver.GetFormatterWithVerify<T18>().Serialize(ref writer, value.Item18, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;
            var item16 = default16;
            var item17 = default17;
            var item18 = default18;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    case 15:
                        item16 = resolver.GetFormatterWithVerify<T16>().Deserialize(ref reader, options);
                        break;
                    case 16:
                        item17 = resolver.GetFormatterWithVerify<T17>().Deserialize(ref reader, options);
                        break;
                    case 17:
                        item18 = resolver.GetFormatterWithVerify<T18>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;
        [Key(15)]
        public readonly T16 Item16;
        [Key(16)]
        public readonly T17 Item17;
        [Key(17)]
        public readonly T18 Item18;
        [Key(18)]
        public readonly T19 Item19;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(19);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
            resolver.GetFormatterWithVerify<T16>().Serialize(ref writer, value.Item16, options);
            resolver.GetFormatterWithVerify<T17>().Serialize(ref writer, value.Item17, options);
            resolver.GetFormatterWithVerify<T18>().Serialize(ref writer, value.Item18, options);
            resolver.GetFormatterWithVerify<T19>().Serialize(ref writer, value.Item19, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;
            var item16 = default16;
            var item17 = default17;
            var item18 = default18;
            var item19 = default19;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    case 15:
                        item16 = resolver.GetFormatterWithVerify<T16>().Deserialize(ref reader, options);
                        break;
                    case 16:
                        item17 = resolver.GetFormatterWithVerify<T17>().Deserialize(ref reader, options);
                        break;
                    case 17:
                        item18 = resolver.GetFormatterWithVerify<T18>().Deserialize(ref reader, options);
                        break;
                    case 18:
                        item19 = resolver.GetFormatterWithVerify<T19>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19);
        }
    }
    
    [MessagePackObject]
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>
    {
        [Key(0)]
        public readonly T1 Item1;
        [Key(1)]
        public readonly T2 Item2;
        [Key(2)]
        public readonly T3 Item3;
        [Key(3)]
        public readonly T4 Item4;
        [Key(4)]
        public readonly T5 Item5;
        [Key(5)]
        public readonly T6 Item6;
        [Key(6)]
        public readonly T7 Item7;
        [Key(7)]
        public readonly T8 Item8;
        [Key(8)]
        public readonly T9 Item9;
        [Key(9)]
        public readonly T10 Item10;
        [Key(10)]
        public readonly T11 Item11;
        [Key(11)]
        public readonly T12 Item12;
        [Key(12)]
        public readonly T13 Item13;
        [Key(13)]
        public readonly T14 Item14;
        [Key(14)]
        public readonly T15 Item15;
        [Key(15)]
        public readonly T16 Item16;
        [Key(16)]
        public readonly T17 Item17;
        [Key(17)]
        public readonly T18 Item18;
        [Key(18)]
        public readonly T19 Item19;
        [Key(19)]
        public readonly T20 Item20;

        [SerializationConstructor]
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

        public void Serialize(ref MessagePackWriter writer, DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(20);
            var resolver = options.Resolver;
            resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Item1, options);
            resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Item2, options);
            resolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Item3, options);
            resolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Item4, options);
            resolver.GetFormatterWithVerify<T5>().Serialize(ref writer, value.Item5, options);
            resolver.GetFormatterWithVerify<T6>().Serialize(ref writer, value.Item6, options);
            resolver.GetFormatterWithVerify<T7>().Serialize(ref writer, value.Item7, options);
            resolver.GetFormatterWithVerify<T8>().Serialize(ref writer, value.Item8, options);
            resolver.GetFormatterWithVerify<T9>().Serialize(ref writer, value.Item9, options);
            resolver.GetFormatterWithVerify<T10>().Serialize(ref writer, value.Item10, options);
            resolver.GetFormatterWithVerify<T11>().Serialize(ref writer, value.Item11, options);
            resolver.GetFormatterWithVerify<T12>().Serialize(ref writer, value.Item12, options);
            resolver.GetFormatterWithVerify<T13>().Serialize(ref writer, value.Item13, options);
            resolver.GetFormatterWithVerify<T14>().Serialize(ref writer, value.Item14, options);
            resolver.GetFormatterWithVerify<T15>().Serialize(ref writer, value.Item15, options);
            resolver.GetFormatterWithVerify<T16>().Serialize(ref writer, value.Item16, options);
            resolver.GetFormatterWithVerify<T17>().Serialize(ref writer, value.Item17, options);
            resolver.GetFormatterWithVerify<T18>().Serialize(ref writer, value.Item18, options);
            resolver.GetFormatterWithVerify<T19>().Serialize(ref writer, value.Item19, options);
            resolver.GetFormatterWithVerify<T20>().Serialize(ref writer, value.Item20, options);
        }

        public DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;
            var length = reader.ReadArrayHeader();

            var item1 = default1;
            var item2 = default2;
            var item3 = default3;
            var item4 = default4;
            var item5 = default5;
            var item6 = default6;
            var item7 = default7;
            var item8 = default8;
            var item9 = default9;
            var item10 = default10;
            var item11 = default11;
            var item12 = default12;
            var item13 = default13;
            var item14 = default14;
            var item15 = default15;
            var item16 = default16;
            var item17 = default17;
            var item18 = default18;
            var item19 = default19;
            var item20 = default20;

            for (var i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        item1 = resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        item2 = resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        item3 = resolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        item4 = resolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        item5 = resolver.GetFormatterWithVerify<T5>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        item6 = resolver.GetFormatterWithVerify<T6>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        item7 = resolver.GetFormatterWithVerify<T7>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        item8 = resolver.GetFormatterWithVerify<T8>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        item9 = resolver.GetFormatterWithVerify<T9>().Deserialize(ref reader, options);
                        break;
                    case 9:
                        item10 = resolver.GetFormatterWithVerify<T10>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        item11 = resolver.GetFormatterWithVerify<T11>().Deserialize(ref reader, options);
                        break;
                    case 11:
                        item12 = resolver.GetFormatterWithVerify<T12>().Deserialize(ref reader, options);
                        break;
                    case 12:
                        item13 = resolver.GetFormatterWithVerify<T13>().Deserialize(ref reader, options);
                        break;
                    case 13:
                        item14 = resolver.GetFormatterWithVerify<T14>().Deserialize(ref reader, options);
                        break;
                    case 14:
                        item15 = resolver.GetFormatterWithVerify<T15>().Deserialize(ref reader, options);
                        break;
                    case 15:
                        item16 = resolver.GetFormatterWithVerify<T16>().Deserialize(ref reader, options);
                        break;
                    case 16:
                        item17 = resolver.GetFormatterWithVerify<T17>().Deserialize(ref reader, options);
                        break;
                    case 17:
                        item18 = resolver.GetFormatterWithVerify<T18>().Deserialize(ref reader, options);
                        break;
                    case 18:
                        item19 = resolver.GetFormatterWithVerify<T19>().Deserialize(ref reader, options);
                        break;
                    case 19:
                        item20 = resolver.GetFormatterWithVerify<T20>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18, item19, item20);
        }
    }
}