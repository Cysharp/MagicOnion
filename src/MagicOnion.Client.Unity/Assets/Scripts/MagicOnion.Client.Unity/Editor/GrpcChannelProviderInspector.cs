using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicOnion.Unity.Editor
{
    [CustomEditor(typeof(GrpcChannelProviderHost))]
    public class GrpcChannelProviderInspector : UnityEditor.Editor
    {
        private GrpcChannelProviderMonitor _monitor;

        public override bool RequiresConstantRepaint()
        {
            return _monitor?.TryUpdate() ?? false;
        }

        public override void OnInspectorGUI()
        {
            var providerHost = (GrpcChannelProviderHost)target;
            if (providerHost == null || providerHost.Provider == null)
            {
                _monitor = null;
                return;
            }

            if (_monitor == null)
            {
                _monitor = new GrpcChannelProviderMonitor(providerHost.Provider);
            }

            _monitor.DrawChannels();
        }
    }
}