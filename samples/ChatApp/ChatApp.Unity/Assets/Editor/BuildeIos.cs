#if UNITY_IPHONE
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class BuildIos
{
    /// <summary>
    /// Handle libgrpc project settings.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="path"></param>
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        var projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));
        var targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());

        // libz.tbd for grpc ios build
        project.AddFrameworkToProject(targetGuid, "libz.tbd", false);

        // libgrpc_csharp_ext missing bitcode. as BITCODE exand binary size to 250MB.
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        File.WriteAllText(projectPath, project.WriteToString());
    }
}
#endif