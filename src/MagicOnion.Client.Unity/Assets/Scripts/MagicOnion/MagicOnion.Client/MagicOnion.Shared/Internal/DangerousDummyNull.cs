using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MagicOnion.Internal
{
    internal class DangerousDummyNull
    {
        public static DangerousDummyNull Instance { get; } = new DangerousDummyNull();

        DangerousDummyNull()
        {}

        public static T GetObjectOrDummyNull<T>(T value)
        {
            if (value is null)
            {
                Debug.Assert(typeof(T).IsClass);
                var instance = Instance;
                return Unsafe.As<DangerousDummyNull, T>(ref instance);
            }

            return value;
        }

        public static T GetObjectOrDefault<T>(object value)
        {
            if (object.ReferenceEquals(value, Instance))
            {
                return default(T)!;
            }

            return (T)value;
        }
    }
}

