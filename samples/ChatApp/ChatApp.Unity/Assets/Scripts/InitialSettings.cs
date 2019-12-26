using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace Assets.Scripts
{
    class InitialSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterResolvers()
        {
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
                .WithResolver(
                    CompositeResolver.Create(
                        MagicOnion.Resolvers.MagicOnionResolver.Instance,
                        MessagePack.Resolvers.GeneratedResolver.Instance,
                        BuiltinResolver.Instance,
                        PrimitiveObjectResolver.Instance
                    )
                );
        }
    }
}
