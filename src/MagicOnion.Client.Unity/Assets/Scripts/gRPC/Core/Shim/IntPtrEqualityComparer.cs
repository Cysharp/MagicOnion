using System;
using System.Collections.Generic;

namespace Grpc.Core
{
    internal class IntPtrEqualityComparer : IEqualityComparer<IntPtr>
    {
        public static readonly IntPtrEqualityComparer Instance = new IntPtrEqualityComparer();

        IntPtrEqualityComparer()
        {

        }

        public bool Equals(IntPtr x, IntPtr y)
        {
            return (x == y);
        }

        public int GetHashCode(IntPtr obj)
        {
            return obj.GetHashCode();
        }
    }
}
