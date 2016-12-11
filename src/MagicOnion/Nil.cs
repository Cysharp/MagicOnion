using System;
using ZeroFormatter;

namespace MagicOnion
{
    /// <summary>
    /// Represents Void/Unit.
    /// </summary>
    [ZeroFormattable]
    public struct Nil : IEquatable<Nil>
    {
        public static readonly Nil Default = default(Nil);

        public bool Equals(Nil other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}