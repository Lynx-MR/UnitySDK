/**
 * @file SampleImporter.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Automatize required samples importation in one clic from Editor menu.
 */

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Lynx
{
    public class SampleImporter
    {
        private static (string, string) STARTER_ASSETS_PATH = ("Packages/com.unity.xr.interaction.toolkit", "Starter Assets");

        public static void ImportPackage(string pkgPath, string sampleName)
        {
            Debug.Log(pkgPath);
            string samplePath = Path.Combine(pkgPath, "Samples~", sampleName);
            if (!Directory.Exists(samplePath))
            {
                Debug.LogError($"Sample missing {samplePath}");
                return;
            }

            UnityEditor.PackageManager.PackageInfo info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(pkgPath);
            Debug.Log(info);
            Debug.Log($"Package:{info.name} \"{info.displayName}\" {info.version}");

            string dstSample = Path.Combine(Application.dataPath, "Samples", info.displayName, info.version, sampleName);
            if (!Directory.Exists(dstSample))
                Directory.CreateDirectory(dstSample);

            FileUtil.ReplaceDirectory(Path.Combine(info.resolvedPath, "Samples~", sampleName), dstSample);

        }

        [MenuItem("Lynx/Import Starter Assets (required XRI Sample)", false, 300)]
        public static void ImportStarterAssets()
        {
            ImportPackage(STARTER_ASSETS_PATH.Item1, STARTER_ASSETS_PATH.Item2);
        }
    }
}