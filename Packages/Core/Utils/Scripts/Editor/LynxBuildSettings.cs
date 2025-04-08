﻿/**
 * @file LynxBuildSettings.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Automatically manage the Android settings to match with Lynx-R1 device.
 */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lynx
{
    public class LynxBuildSettings
    {
        public const string LYNX_CORE_PATH = "Packages/com.lynx.core";

        /// <summary>
        /// Configure project to target Lynx headset before Unity 6
        /// </summary>
        public static void SetupAndroidBuild()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

            PlayerSettings.MTRendering = true;

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetMobileMTRendering(UnityEditor.Build.NamedBuildTarget.Android, true);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel32;

#else
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
#endif

            PlayerSettings.colorSpace = ColorSpace.Linear;

            if (PlayerSettings.Android.disableDepthAndStencilBuffers)
                Debug.LogWarning("WARNING: Depth and Stencil is disabled. If screen stays black, you should enable it back.");
        }

        /// <summary>
        /// Retrieve all object even disabled from the scene.
        /// </summary>
        /// <typeparam name="T">Object type to retrieve.</typeparam>
        /// <returns>List of all T objects in the scene.</returns>
        public static List<T> FindObjectsOfTypeAll<T>()
        {
            List<T> results = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    var allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        var go = allGameObjects[j];
                        results.AddRange(go.GetComponentsInChildren<T>(true));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Search and instantiate a gameobject having the given path in name.
        /// It will be attached to the given transform parent.
        /// </summary>
        /// <param name="path">Object name to instantiate.</param>
        /// <param name="parent">Transform used as parent for the new gameobject</param>
        /// <returns></returns>
        public static GameObject InstantiateGameObjectByPath(string packagePath, string path, Transform parent)
        {

            string[] paths = Directory.GetFiles(packagePath, path, SearchOption.AllDirectories);

            // File does not exists (probably due to missing required dependencies)
            if(paths.Length == 0)
                return null;
            

            GameObject obj;
            if(packagePath == Application.dataPath)
                obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<Object>("Assets" + paths[0].Substring(Application.dataPath.Length)), null) as GameObject;
            else
                obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<Object>(paths[0]), null) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return obj;
        }
    }
}