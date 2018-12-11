#if NON_UNITY

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MagicOnion.Utils
{
    public class DuplicateKeyException : Exception
    {
        public int HashCode { get; }

        public DuplicateKeyException(int hashCode)
            : base("Duplicated. HashCode:" + hashCode)
        {
            HashCode = hashCode;
        }
    }

    /*
        < 10000 String.GetHashCode is almost unique.
        For example, Class/Methods lookup can avoid raw string lookup.
    */
    public class UniqueHashDictionary<T>
    {
        (int hash, T value)[][] table;
        int indexFor;
        int count;
        int maxConflict; // debugging for perf.

        public int Count => count;
        public int MaxConflict => maxConflict;

        public UniqueHashDictionary(params (int hashCode, T value)[] values)
        {
            CreateTable(values);
        }

        void CreateTable((int hashCode, T value)[] values)
        {
            var capacity = CalculateCapacity(values.Length, 0.72f);

            table = new (int, T)[capacity][];
            indexFor = table.Length - 1;
            count = values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                ref var v = ref values[i];

                var index = v.hashCode & indexFor;
                if (table[index] == null)
                {
                    // first case.
                    table[index] = new (int, T)[1] { v };
                    continue;
                }
                else
                {
                    ref var t = ref table[index];

                    // check duplicate
                    foreach (var item in t)
                    {
                        if (item.hash == v.hashCode)
                        {
                            throw new DuplicateKeyException(v.hashCode);
                        }
                    }

                    // add last
                    Array.Resize(ref t, t.Length + 1);
                    t[t.Length - 1] = v;

                    maxConflict = Math.Max(maxConflict, t.Length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int hashCode, out T value)
        {
            ref var array = ref table[hashCode & indexFor];
            if (array == null)
            {
                value = default(T);
                return false;
            }

            ref var v = ref array[0];
            if (v.hash == hashCode)
            {
                value = v.value;
                return true;
            }

            return TryGetValueSlow(ref array, hashCode, out value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool TryGetValueSlow(ref (int, T)[] array, int hashCode, out T value)
        {
            for (int i = 1; i < array.Length; i++) // 0 is already checked.
            {
                ref var v = ref array[i];
                if (v.Item1 == hashCode)
                {
                    value = v.Item2;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        public IEnumerable<T> AllValues()
        {
            foreach (var item in table)
            {
                if (item != null)
                {
                    foreach (var item2 in item)
                    {
                        yield return item2.value;
                    }
                }
            }
        }

        static int CalculateCapacity(int collectionSize, float loadFactor)
        {
            var size = (int)(((float)collectionSize) / loadFactor);

            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            size += 1;

            // specialize adjust
            if (size < 128)
            {
                size = 128;
            }
            return size;
        }

        public override string ToString()
        {
            return $"Count:{Count} MaxConflict:{MaxConflict}";
        }
    }
}

#endif