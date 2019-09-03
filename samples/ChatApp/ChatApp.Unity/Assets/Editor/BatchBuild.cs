#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BatchBuild
{
    public static void Build()
    {
        SetAndroidPath();
        var option = GetBuildPlayerOptionFromCommandLineArgs();
        option.scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        var buildReport = BuildPipeline.BuildPlayer(option);

        Debug.Log($@"Build {buildReport.summary.result}, Errors {buildReport.summary.totalErrors}, Warnings {buildReport.summary.totalWarnings}
TotalTime {buildReport.summary.totalTime}, Size {buildReport.summary.totalSize}
OutputPath {buildReport.summary.outputPath}");
        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }

    private static BuildPlayerOptions GetBuildPlayerOptionFromCommandLineArgs()
    {
        var option = new BuildPlayerOptions();
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-platform":
                    {
                        if (Enum.TryParse<BuildTarget>(args[i + 1], true, out var p))
                        {
                            option.target = p;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException($"-platform {args[i + 1]} should match to {string.Join(",", Enum.GetNames(typeof(BuildTarget)))}");
                        }
                    }
                    break;
                case "-development":
                    // development + allow profiler debug
                    option.options |= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
                    break;
                case "-locationpath":
                    option.locationPathName = args[i + 1];
                    break;
                default:
                    break;
            }
        }

        // fallback locationPath when argument missing
        if (string.IsNullOrWhiteSpace(option.locationPathName))
        {
            var projectName = PlayerSettings.productName;
            var extension = option.target == BuildTarget.Android ? ".apk"
                : option.target == BuildTarget.StandaloneWindows64 || option.target == BuildTarget.StandaloneWindows ? ".exe"
                : "";
            option.locationPathName = $"./build/{option.target.ToString().ToLower()}/{projectName}{extension}";
        }

        return option;
    }

    private static void SetAndroidPath()
    {
        var sdk = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "YOUR DEFAULT PATH to SDK (/usr/local/share/android-sdk)";
        var ndk = Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT") ?? "YOUR DEFAULT PATH to NDK (/usr/local/share/android-sdk/ndk-bundle)";
        Debug.Log($"sdk: {sdk}");
        Debug.Log($"ndk: {ndk}");
        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRootR16b", ndk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);
    }
}
#endif