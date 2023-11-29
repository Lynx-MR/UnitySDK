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
        static AddRequest req = null; // Request handler for package installation

        public const string ULTRALEAP_GIT_URL = "https://github.com/ultraleap/UnityPlugin.git?path=/Packages/Tracking";
        public const string ULTRALEAP_PACKAGE_NAME = "com.ultraleap.tracking";


        /// <summary>
        /// To start packages installation
        /// </summary>
        [MenuItem("Lynx/Settings/Install Ultraleap Package", false, 100)]
        static void InstallUltraleapPackage()
        {
            EditorApplication.LockReloadAssemblies();
            req = Client.Add(ULTRALEAP_GIT_URL);
            Debug.Log($"Installing {ULTRALEAP_PACKAGE_NAME}...");
            EditorApplication.update += Progress;

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
                        // Refresh Unity
                        EditorApplication.update -= Progress;
                        EditorApplication.UnlockReloadAssemblies();
                    }

                }
                else
                {
                    Debug.Log($"Failed");

                }

                // Refresh Unity
                EditorApplication.update -= Progress;
                EditorApplication.UnlockReloadAssemblies();

                AssetDatabase.Refresh();
                EditorUtility.RequestScriptReload();
            }
        }
    }
}