using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;

namespace MagicOnion.Unity
{
    /// <summary>
    /// A host that integrates gRPC channel management with Unity lifecycle.
    /// </summary>
    /// <example>
    /// <code>
    /// // Initialize gRPC channel provider when the application is loaded.
    /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// public static void OnRuntimeInitialize()
    /// {
    ///     GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(new []
    ///     {
    ///         new ChannelOption("grpc.keepalive_time_ms", 5000)
    ///     }));
    /// }
    /// </code>
    /// </example>
    public class GrpcChannelProviderHost : MonoBehaviour
    {
        public IGrpcChannelProvider Provider { get; private set; }

        /// <summary>
        /// Initialize gRPC channel provider and the host.
        /// </summary>
        /// <param name="provider"></param>
        public static void Initialize(IGrpcChannelProvider provider)
        {
            foreach (var instance in GameObject.FindObjectsOfType<GrpcChannelProviderHost>())
            {
                if (instance.gameObject != null)
                {
                    GameObject.Destroy(instance.gameObject);
                }
            }

            // Create a new GrpcChannelProvider and set the provider as a default.
            var go = new GameObject("GrpcChannelProvider");
            var providerHost = go.AddComponent<GrpcChannelProviderHost>();
            GameObject.DontDestroyOnLoad(go);

            providerHost.InitializeCore(provider);
        }

        private void InitializeCore(IGrpcChannelProvider provider)
        {
            Provider = provider ?? new DefaultGrpcChannelProvider();
            GrpcChannelProvider.SetDefaultProvider(Provider);
        }

        private void OnDestroy()
        {
            Provider.ShutdownAllChannels();
        }

        private void OnApplicationQuit()
        {
            Provider.ShutdownAllChannels();
        }
    }
}