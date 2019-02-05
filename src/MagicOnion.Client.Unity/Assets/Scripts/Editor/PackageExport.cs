using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityEditorUtility
{
    public static class PackageExport
    {
        [MenuItem("Tools/Export Unitypackage")]
        public static void Export()
        {
            // configure
            var root = "Scripts/MagicOnion";
            var exportPath = "../../nuget/MagicOnion.Unity.Version-Rename.unitypackage";

            var path = Path.Combine(Application.dataPath, root);
            var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Where(x => Path.GetExtension(x) == ".cs")
                .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
                .ToArray();

            UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

            AssetDatabase.ExportPackage(
                assets,
                exportPath,
                ExportPackageOptions.Default);

            UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
        }
    }
}
