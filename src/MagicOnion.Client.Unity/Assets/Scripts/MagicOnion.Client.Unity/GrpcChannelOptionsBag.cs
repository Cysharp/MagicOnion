using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MagicOnion.Unity
{
    public class GrpcChannelOptionsBag
    {
        readonly object? options;

        public GrpcChannelOptionsBag(object? options)
        {
            this.options = options;
        }

        public T? GetOrDefault<T>()
        {
            return TryGet<T>(out var value) ? value : default;
        }

        public bool TryGet<T>([NotNullWhen(true)] out T? value)
        {
            if (options is T optionT)
            {
                value = optionT;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            if (TryGet<IChannelOptionsValueProvider>(out var valueProvider))
            {
                foreach (var keyValue in valueProvider.GetValues())
                {
                    yield return keyValue;
                }
            }
        }
    }

    public interface IChannelOptionsValueProvider
    {
        IEnumerable<KeyValuePair<string, object>> GetValues();
    }
}
