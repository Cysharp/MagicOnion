#if UNITY_EDITOR
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
            }
        }

        static void PlayModeChanged()
        {
            if (EditorApplication.isPlaying)
                return;

            EditorApplication.UnlockReloadAssemblies();
            EditorApplication.playmodeStateChanged -= PlayModeChanged;
            isStopped = false;
        }
    }
}
#endif