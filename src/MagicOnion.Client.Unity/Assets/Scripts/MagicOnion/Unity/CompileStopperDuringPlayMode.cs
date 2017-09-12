#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MagicOnion
{
    [InitializeOnLoad]
    public class CompileStopperDuringPlayMode
    {
        static bool isStopped = false;

        static CompileStopperDuringPlayMode()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnEditorUpdate()
        {
            if (!isStopped
                && EditorApplication.isCompiling
                && EditorApplication.isPlaying)
            {
                EditorApplication.LockReloadAssemblies();
                EditorApplication.playmodeStateChanged += PlayModeChanged;
                isStopped = true;
                Debug.Log("Stop Compiling the scripts");
            }
        }

        static void PlayModeChanged()
        {
            if (EditorApplication.isPlaying)
                return;

            EditorApplication.UnlockReloadAssemblies();
            EditorApplication.playmodeStateChanged -= PlayModeChanged;
            isStopped = false;
            Debug.Log("Start Compiling the scripts");
        }
    }
}
#endif