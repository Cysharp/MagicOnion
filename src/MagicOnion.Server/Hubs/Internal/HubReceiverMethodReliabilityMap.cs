using System.Collections.Frozen;
using System.Reflection;
using MagicOnion.Internal;

namespace MagicOnion.Server.Hubs.Internal;

internal record HubReceiverMethodReliabilityMap(
    FrozenDictionary<int, TransportReliability> ReliabilityByMethodId,
    bool IsSingleReliability,
    TransportReliability DefaultReliability
)
{
    public static HubReceiverMethodReliabilityMap Create<TReceiver>()
    {
        var targetType = typeof(TReceiver);
        var defaultReliability = TransportReliability.Reliable;
        var reliabilityForType = targetType.GetCustomAttribute<TransportAttribute>()?.Reliability ?? defaultReliability;
        var reliabilityByMethods = targetType.GetMethods()
            .Concat(targetType.GetInterfaces().SelectMany(x => x.GetMethods()))
            .Select(x => (x.Name, x.ReturnType, Reliability: x.GetCustomAttribute<TransportAttribute>()?.Reliability ?? reliabilityForType))
            .ToDictionary(k => k.Name, v => (v.Reliability, v.ReturnType, MethodId: FNV1A32.GetHashCode(v.Name))); // TODO: MethodIdAttribute

        // Validate
        if (reliabilityByMethods.FirstOrDefault(x => IsAwaitable(x.Value.ReturnType) && x.Value.Reliability != TransportReliability.Reliable) is { Key: not null } method)
        {
            throw new NotSupportedException($"Method '{method.Key}' has awaitable type. but the client result method must be Reliable");
        }

        if (reliabilityByMethods.Select(x => x.Value.Reliability).ToHashSet() is { } reliabilities && reliabilities.Count <= 1)
        {
            // All methods of HubReceiver are same reliability
            return new HubReceiverMethodReliabilityMap(
                reliabilityByMethods.ToFrozenDictionary(k => k.Value.MethodId, v => v.Value.Reliability),
                IsSingleReliability: true,
                DefaultReliability: reliabilities.Count == 0 ? defaultReliability : reliabilities.First());
        }
        else
        {
            // HubReceiver has multiple reliabilities.
            return new HubReceiverMethodReliabilityMap(
                reliabilityByMethods.ToFrozenDictionary(k => k.Value.MethodId, v => v.Value.Reliability),
                IsSingleReliability: false,
                DefaultReliability: defaultReliability);
        }

        static bool IsAwaitable(Type t)
        {
            return t == typeof(Task) ||
                   t == typeof(ValueTask) ||
                   (t.IsGenericType && t.GetGenericTypeDefinition() is { } openType && (openType == typeof(Task<>) || openType == typeof(ValueTask<>)));
        }
    }
}
