/**
 * @file PackagesInstaller.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Automatically manage packages for lynx-r dependencies.
 */


using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Lynx
{
    public class PackagesInstaller
    {
        static List<string> samples_blacklist= new List<string>();
        static AddRequest req = null; // Request handler for package installation
        private static int idx = 0;

        private struct PackageToInstall
        {
            public string name;
            public string packageName;
            public string url;
            public string[] samples;
        }

        // Packages to install
        private static readonly List<PackageToInstall> PackagesAndSamples = new List<PackageToInstall> {
            new PackageToInstall(){ name="Open XR", packageName =  "com.unity.xr.openxr", url =  "com.unity.xr.openxr", samples = new string[] { } },
            new PackageToInstall(){ name="XR Hands", packageName =  "com.unity.xr.hands", url =  "com.unity.xr.hands", samples = new string[] { "HandVisualizer" } },
            new PackageToInstall(){ name="XR Interaction Toolkit", packageName =  "com.unity.xr.interaction.toolkit", url =  "com.unity.xr.interaction.toolkit", samples = new string[] { "Starter Assets", "Hands Interaction Demo" } },
            new PackageToInstall(){ name="Ultraleap Tracking", packageName =  "com.ultraleap.tracking", url =  "https://github.com/ultraleap/UnityPlugin.git?path=/Packages/Tracking", samples = new string[] { } }
        };


        /// <summary>
        /// Install required samples
        /// </summary>
        //[MenuItem("Lynx/Samples/Import XRI Samples", false, 101)] // Not ready yet, create conflict with XRI samples default import
        static void InstallSamples()
        {

            EditorApplication.LockReloadAssemblies();

            // Create Samples folder in Assets/ if required
            const string samplesPath = "Assets/Samples";
            if (!Directory.Exists(samplesPath))
                Directory.CreateDirectory(samplesPath);


            for (int idxPackage = 0, countPackages = PackagesAndSamples.Count; idxPackage < countPackages; ++idxPackage)
            {
                PackageToInstall package = PackagesAndSamples[idxPackage];
                if (package.samples.Length > 0)
                {
                    Debug.Log($"Import {package.name} samples...");
                    string assetsSamplesPath = $"{samplesPath}/{package.name}";
                    if (!Directory.Exists(assetsSamplesPath))
                        Directory.CreateDirectory(assetsSamplesPath);

                    string packageSamplesPath = $"Packages/{package.packageName}/Samples~";
                    for (int idxSample = 0, countSamples = package.samples.Length; idxSample < countSamples; ++idxSample)
                    {
                        string path = $"{packageSamplesPath}/{package.samples[idxSample]}";
                        if (Directory.Exists(path))
                        {
                            Debug.Log($"\t{package.samples[idxSample]}");
                            FileUtil.CopyFileOrDirectory(path, $"{assetsSamplesPath}/{package.samples[idxSample]}");
                        }
                    }
                }
            }

            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
        }



        /// <summary>
        /// Install next package in the list
        /// </summary>
        private static void InstallNextPackage()
        {

            PackageToInstall package = PackagesAndSamples[idx++];
            Debug.Log($"Installing {package.packageName}...");
            req = Client.Add(package.url);
        }


        /// <summary>
        /// To start packages installation
        /// </summary>
        [MenuItem("Lynx/Settings/Install packages", false, 100)]
        static void InstallAllPackagesUI()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(PackagesInstallationWindow));
            Rect r = window.position;
            r.position = new Vector2(150.0f, 150.0f);
            r.width = 400.0f;
            r.height = 200.0f;
            window.position = r;


        }

        /// <summary>
        /// Start to install packages and samples
        /// </summary>
        static void InstallAllPackages()
        {
            idx = 0;

            // Lock and start checking end of download
            EditorApplication.LockReloadAssemblies();
            EditorApplication.update += Progress;

            // Go to first package to install
            InstallNextPackage();
        }

        /// <summary>
        /// At each update, check if current package request is installed. Then try to install next package.
        /// If there is no other package to install, do not look for completed request at each update and unlock reload assemblies to refresh Unity Editor once.
        /// </summary>
        static void Progress()
        {
            if (req.IsCompleted)
            {
                if (req.Status == StatusCode.Success)
                {
                    try
                    {
                        Debug.Log($"Installed: {req.Result.packageId}");
                        IEnumerable<UnityEditor.PackageManager.UI.Sample> samples = UnityEditor.PackageManager.UI.Sample.FindByPackage(req.Result.displayName, req.Result.version);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }

                }
                else if (req.Status >= StatusCode.Failure)
                    Debug.Log(req.Error.message);

                if (idx < PackagesAndSamples.Count)
                {
                    InstallNextPackage();
                }
                else
                {
                    EditorApplication.update -= Progress;
                    idx = 0;
                    EditorApplication.UnlockReloadAssemblies();

                    AssetDatabase.Refresh();
                    EditorUtility.RequestScriptReload();
                }
            }
        }

        public class PackagesInstallationWindow : EditorWindow
        {
            bool isHandVisualiserSamplePresent = true;
            bool isStarterAssetPresent = true;
            void OnGUI()
            {
                GUILayout.BeginVertical();
                GUILayout.Space(20);
                GUILayout.Label("The following packages will be installed:\n\n" +
                    "- Open XR\n" +
                    "- XR Interaction Toolkit\n" +
                    "- XR Hands\n" +
                    "- Ultraleap Handtracking\n", EditorStyles.label);

                GUILayout.EndVertical();


                if (GUILayout.Button("Validate"))
                {
                    samples_blacklist.Clear();
                    if (!isStarterAssetPresent)
                        samples_blacklist.Add("Starter Assets");

                    if (!isHandVisualiserSamplePresent)
                        samples_blacklist.Add("HandVisualizer");

                    InstallAllPackages();
                    this.Close();
                }


                if (GUILayout.Button("Cancel"))
                {
                    this.Close();
                }

            }
        }
    }
}