







using System;
using System.Runtime.InteropServices;
using MessagePack;
using MessagePack.Formatters;

namespace MagicOnion
{
    // T2 ~ T15
    // NOTE: Blazor WebAssembly (AOT) does not support more than 16 generic type parameters.


    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2>
#else
    public struct DynamicArgumentTuple<T1, T2>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3>
#else
    public struct DynamicArgumentTuple<T1, T2, T3>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
#endif
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

    
    [MessagePackObject]
    [StructLayout(LayoutKind.Auto)]
#if MAGICONION_USE_REFTYPE_DYNAMICARGUMENTTUPLE
    public sealed class DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
#else
    public struct DynamicArgumentTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
#endif
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

}
