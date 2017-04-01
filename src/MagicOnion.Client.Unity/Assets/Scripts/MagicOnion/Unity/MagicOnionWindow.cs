#if UNITY_EDITOR

using Grpc.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniRx;

namespace MagicOnion
{
    public class MagicOnionWindow : EditorWindow
    {
        [MenuItem("Window/MagicOnion")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<MagicOnionWindow>("MagicOnion");
            window.Show();
        }

        static List<Channel> connectionInfos = new List<Channel>();
        static List<string> unregistredSubscriptions = new List<string>();
        static Dictionary<string, KeyValuePair<WeakReference, string>> subscriptions = new Dictionary<string, KeyValuePair<WeakReference, string>>();

        Vector2 scrollPosition = Vector2.zero;
        long updateCallCount = 0;

        void Update()
        {
            if (updateCallCount++ % 100 == 0)
            {
                Repaint(); // call OnGUI
            }
        }

        public static string AddSubscription(Channel channel, string method)
        {
            lock (subscriptions)
            {
                var key = System.Guid.NewGuid().ToString();

                var channelRef = new WeakReference(channel);
                subscriptions.Add(key, new KeyValuePair<WeakReference, string>(channelRef, method));

                return key;
            }
        }

        public static void RemoveSubscription(string id)
        {
            lock (subscriptions)
            {
                subscriptions.Remove(id);
            }
        }

        private void OnGUI()
        {
            var count = GrpcEnvironment.GetCurrentChannels(connectionInfos);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("gRPC Channels(" + count + ")", EditorStyles.boldLabel);

                for (int i = 0; i < count; i++)
                {
                    var channel = connectionInfos[i];

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(channel.Target);
                        if (GUILayout.Button("Shutdown"))
                        {
                            channel.ShutdownAsync().Subscribe();
                        }
                    }

                    EditorGUI.indentLevel += 1;
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PrefixLabel("State");
                            EditorGUILayout.LabelField(ChannelStateToString(channel.State));
                        }
                    }

                    lock (subscriptions)
                    {
                        foreach (var item in subscriptions)
                        {
                            var c = item.Value.Key.Target;
                            if (channel == c)
                            {
                                EditorGUILayout.LabelField(item.Value.Value);
                            }
                        }
                    }

                    EditorGUI.indentLevel -= 1;
                }

                lock (subscriptions)
                {
                    foreach (var item in subscriptions)
                    {
                        var c = item.Value.Key.Target;
                        if (c != null)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var channel = connectionInfos[i];
                                //channel
                                if (c == channel)
                                {
                                    goto next;
                                }
                            }

                            unregistredSubscriptions.Add(item.Value.Value);
                            continue;
                        }

                        next:
                        continue;
                    }
                }

                if (unregistredSubscriptions.Count != 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Unfinished Subscriptions", EditorStyles.boldLabel);

                    for (int i = 0; i < unregistredSubscriptions.Count; i++)
                    {
                        var m = unregistredSubscriptions[i];
                        EditorGUILayout.LabelField(m);
                    }

                    unregistredSubscriptions.Clear();
                }
            }
            EditorGUILayout.EndScrollView();

            for (int i = 0; i < connectionInfos.Count; i++)
            {
                connectionInfos[i] = null; // null clear
            }
        }

        string ChannelStateToString(ChannelState state)
        {
            switch (state)
            {
                case ChannelState.Idle:
                    return "Idle";
                case ChannelState.Connecting:
                    return "Connecting";
                case ChannelState.Ready:
                    return "Ready";
                case ChannelState.TransientFailure:
                    return "TransientFailure";
                case ChannelState.Shutdown:
                    return "Shutdown";
                default:
                    return state.ToString();
            }
        }
    }
}

#endif