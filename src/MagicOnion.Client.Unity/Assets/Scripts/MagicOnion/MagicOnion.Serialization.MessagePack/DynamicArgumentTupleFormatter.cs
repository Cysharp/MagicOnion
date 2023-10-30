
using System;
using System.Runtime.InteropServices;
using MessagePack;
using MessagePack.Formatters;

namespace MagicOnion.Serialization.MessagePack
{
    // T2 ~ T15
    // NOTE: Blazor WebAssembly (AOT) does not support more than 16 generic type parameters.


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
}
