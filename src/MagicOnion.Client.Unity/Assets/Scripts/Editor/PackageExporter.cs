using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PackageExporter
{
    [MenuItem("Tools/Export Unitypackage")]
    public static void Export()
    {
        var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");

        // configure
        var roots = new[]
        {
            "Scripts/MagicOnion.Client",
            "Scripts/MagicOnion.Abstractions",
            "Scripts/MagicOnion.Unity",
        };
        var fileName = string.IsNullOrEmpty(version) ? "MagicOnion.Client.Unity.unitypackage" : $"MagicOnion.Client.Unity.{version}.unitypackage";
        var exportPath = "./" + fileName;

        var packageTargetAssets = roots
            .SelectMany(root =>
            {
                var path = Path.Combine(Application.dataPath, root);
                var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    .Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".asmdef" || Path.GetExtension(x) == ".json" || Path.GetExtension(x) == ".meta")
                    .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
                    .ToArray();
                return assets;
            })
            .ToArray();

        var netStandardsAsset = Directory.EnumerateFiles(Path.Combine(Application.dataPath, "Plugins"), "System.*", SearchOption.AllDirectories)
            .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
            .ToArray();

        packageTargetAssets = packageTargetAssets.Concat(netStandardsAsset).ToArray();

        UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, packageTargetAssets));

        AssetDatabase.ExportPackage(
            packageTargetAssets,
            exportPath,
            ExportPackageOptions.Default);

        UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
    }
}
