using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MagicOnion.Internal
{
    // Pubternal API
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Box<T>
    {
        public readonly T Value;

        public Box(T value)
        {
            Value = value;
        }
    }
}
