/**
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
        /// <summary>
        /// Configure project to target Lynx headset.
        /// </summary>
        public static void SetupAndroidBuild()
        {
#if ULTRALEAP_TRACKING
            // Configure Ultraleap
            if (Leap.Unity.UltraleapSettings.Instance)
                Leap.Unity.UltraleapSettings.Instance.updateMetaInputSystem = true;
            else
                Debug.LogError("Cannot find Ultraleap settings. Please enable Meta Aim Input System manually");
#endif
            
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.MTRendering = true;
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            if (PlayerSettings.Android.disableDepthAndStencilBuffers)
                Debug.LogWarning("WARNING: Depth and Stencil is disabled. If screen stays black, you should enable it back.");

#if ULTRALEAP_TRACKING
            // Configure Ultraleap
            if (Leap.Unity.UltraleapSettings.Instance)
                Leap.Unity.UltraleapSettings.Instance.updateMetaInputSystem = true;
            else
                Debug.LogError("Cannot find Ultraleap settings. Please enable Meta Aim Input System manually");
#endif
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
        public static GameObject InstantiateGameObjectByPath(string path, Transform parent)
        {
            string str_objPath = Directory.GetFiles(Application.dataPath, path, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/");
            GameObject obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<Object>(str_objPath), null) as GameObject;
            obj.transform.parent = parent;
            obj.transform.localPosition = Vector3.zero;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return obj;
        }
    }
}