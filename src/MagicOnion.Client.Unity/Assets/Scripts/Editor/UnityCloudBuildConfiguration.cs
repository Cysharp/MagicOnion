using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

public static class UnityCloudBuildConfiguration
{
    /// <summary>
    /// UnityCloudbuild Post-Export method
    /// </summary>
    /// <param name="exportPath"></param>
    public static void PostBuild(string exportPath)
    {
        // package export
        PackageExporter.Export();

        // gRPC iOS settings
        ApplyGrpcIosSettings(exportPath);
    }

    /// <summary>
    /// Handle gRPC's libgrpc native lib.
    /// </summary>
    /// <param name="exportPath"></param>
    private static void ApplyGrpcIosSettings(string exportPath)
    {
#if UNITY_IPHONE
        var projectPath = PBXProject.GetPBXProjectPath(exportPath);
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));
#if UNITY_2019_3_OR_NEWER
        var targetGuid = project.GetUnityFrameworkTargetGuid();
#else
        var targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif

        // libz.tbd for grpc ios build
        project.AddFrameworkToProject(targetGuid, "libz.tbd", false);

        // // libgrpc_csharp_ext missing bitcode. as BITCODE expand binary size to 250MB.
        project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        
        File.WriteAllText(projectPath, project.WriteToString());
#endif
    }
}
