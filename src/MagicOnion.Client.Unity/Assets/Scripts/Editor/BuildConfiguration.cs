using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class UnityCloudBuildConfiguration
{
#if UNITY_IPHONE
    /// <summary>
    /// Handle libgrpc project settings.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="path"></param>
    public static void PostBuild(string exportPath)
    {
        // package export
        PackageExporter.Export();

        // fix pbx
        var projectPath = PBXProject.GetPBXProjectPath(exportPath);
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));
#if UNITY_2019_3_OR_NEWER
        var targetGuid = project.GetUnityFrameworkTargetGuid();
#else
        var targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
        project.AddFrameworkToProject(targetGuid, "libz.tbd", false);
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        
        File.WriteAllText(projectPath, project.WriteToString());
    }
#endif
}
