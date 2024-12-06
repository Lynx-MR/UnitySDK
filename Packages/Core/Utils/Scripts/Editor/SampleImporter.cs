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
        private static (string, string) HAND_INTERACTIONS_ASSETS_PATH = ("Packages/com.unity.xr.interaction.toolkit", "Hands Interaction Demo");
        private static (string, string) HAND_VISUALIZER_ASSETS_PATH = ("Packages/com.unity.xr.hands", "HandVisualizer");

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

            AssetDatabase.Refresh();

        }

        [MenuItem("Lynx/Import Samples/Import all samples", false, 148)]
        public static void ImportAllSamples()
        {
            ImportPackage(STARTER_ASSETS_PATH.Item1, STARTER_ASSETS_PATH.Item2);
            ImportPackage(HAND_INTERACTIONS_ASSETS_PATH.Item1, HAND_INTERACTIONS_ASSETS_PATH.Item2);
            ImportPackage(HAND_VISUALIZER_ASSETS_PATH.Item1, HAND_VISUALIZER_ASSETS_PATH.Item2);
        }

        [MenuItem("Lynx/Import Samples/XRI - Starter Assets (required)", false, 150)]
        public static void ImportStarterAssets()
        {
            ImportPackage(STARTER_ASSETS_PATH.Item1, STARTER_ASSETS_PATH.Item2);
        }

        [MenuItem("Lynx/Import Samples/XRI - Hands Interaction Demo", false, 151)]
        public static void ImportHandsInteractionDemo()
        {
            ImportPackage(HAND_INTERACTIONS_ASSETS_PATH.Item1, HAND_INTERACTIONS_ASSETS_PATH.Item2);
        }

        [MenuItem("Lynx/Import Samples/XR Hands - HandVisualizer", false, 152)]
        public static void ImportHandVizualiser()
        {
            ImportPackage(HAND_VISUALIZER_ASSETS_PATH.Item1, HAND_VISUALIZER_ASSETS_PATH.Item2);
        }

    }
}