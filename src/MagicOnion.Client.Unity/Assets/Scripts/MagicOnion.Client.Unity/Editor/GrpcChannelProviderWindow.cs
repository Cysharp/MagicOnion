using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicOnion.Unity.Editor
{
    public class GrpcChannelProviderWindow : EditorWindow
    {
        private GrpcChannelProviderMonitor _monitor;
        private Vector2 _scrollPosition;

        [MenuItem("Window/MagicOnion/gRPC Channels")]
        public static void Open()
        {
            ((EditorWindow)EditorWindow.GetWindow<GrpcChannelProviderWindow>()).Show();
        }

        private void Awake()
        {
            this.titleContent = new GUIContent("gRPC Channels");
        }

        private void OnGUI()
        {
            var providerHost = GameObject.FindObjectOfType<GrpcChannelProviderHost>();
            if (providerHost == null)
            {
                EditorGUILayout.HelpBox("Cannot find a gRPC Channel Provider Host", MessageType.Info);
                _monitor = null;
                return;
            }

            if (_monitor == null)
            {
                _monitor = new GrpcChannelProviderMonitor(providerHost.Provider);
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false, new GUILayoutOption[] { }))
            using (new EditorGUILayout.VerticalScope(new GUIStyle()
            {
                padding =
                {
                    left = 10, top = 10,
                    right = 10, bottom = 10,
                }
            }))
            {
                _monitor.DrawChannels();
                _scrollPosition = scope.scrollPosition;
            }
        }

        private void Update()
        {
            if (_monitor?.TryUpdate() ?? false)
            {
                Repaint();
            }
        }
    }
}