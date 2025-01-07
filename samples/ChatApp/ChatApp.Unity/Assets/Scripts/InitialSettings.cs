using System.IO;
using MagicOnion.Client;
using Grpc.Net.Client;
using MagicOnion.Unity;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace Assets.Scripts
{
    [MagicOnionClientGeneration(typeof(ChatApp.Shared.Services.IChatService))]
    partial class MagicOnionClientInitializer {}

    class InitialSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterResolvers()
        {
            // NOTE: Currently, CompositeResolver doesn't work on Unity IL2CPP build. Use StaticCompositeResolver instead of it.
            StaticCompositeResolver.Instance.Register(
                MagicOnionClientInitializer.Resolver,
                StandardResolver.Instance
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
                .WithResolver(StaticCompositeResolver.Instance);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnRuntimeInitialize()
        {
            // Use Grpc.Net.Client instead of C-core gRPC library.
            GrpcChannelProviderHost.Initialize(
                new GrpcNetClientGrpcChannelProvider(() => new GrpcChannelOptions()
                {
                    HttpHandler = new Cysharp.Net.Http.YetAnotherHttpHandler()
                    {
                        Http2Only = true,
                    }
                }));
        }
    }
}
