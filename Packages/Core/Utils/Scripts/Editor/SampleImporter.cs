using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;

namespace Lynx
{
    public class SampleImporter
    {
        static List<(string, string)> packages = new List<(string, string)>()
        {
            ("Packages/com.unity.xr.hands", "HandVisualizer"),
            ("Packages/com.unity.xr.interaction.toolkit", "Starter Assets"),
            ("Packages/com.unity.xr.interaction.toolkit", "Hands Interaction Demo"),
            (LynxBuildSettings.LYNX_CORE_PATH, "UI"),
        };

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



        [MenuItem("Lynx/Import Samples", false, 300)]
        public static void ImportRequiredSamples()
        {
            for(int i=0, count = packages.Count; i<count; ++i)
                ImportPackage(packages[i].Item1, packages[i].Item2);

            AssetDatabase.Refresh();


        }
    }
}