using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    internal static class DictionaryExtensions
    {
#if NETSTANDARD2_0
        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (dict.TryGetValue(key, out var v))
            {
                dict.Remove(key);
                value = v!;
                return true;
            }

            value = default;
            return false;
        }
#endif
    }
}
