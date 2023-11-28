/**
 * @file OpenXRConfiguration.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Add lynx menu in Unity Editor to manage configuration.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if LYNX_OPENXR
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

namespace Lynx.OpenXR
{
    [InitializeOnLoad]
    public class OpenXRConfiguration
    {
        #region EDITOR MENUS
        [MenuItem("Lynx/Settings/Configure project settings", false, 101)]
        public static void ConfigureProject()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(ConfigurationWindow));
            Rect r = window.position;
            r.position = new Vector2(150.0f, 150.0f);
            r.width = 400.0f;
            r.height = 400.0f;
            window.position = r;
        }
        #endregion

        #region METHODS
#if LYNX_OPENXR
        public static void ConfigureOpenXR()
        {
            BuildTargetGroup buildTarget = BuildTargetGroup.Android;

            XRManagerSettings managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            managerSettings.name = "Lynx settings manager";
            OpenXRLoader openXRLoader = ScriptableObject.CreateInstance<OpenXRLoader>();
            managerSettings.TrySetLoaders(new List<XRLoader>() { openXRLoader });

            XRGeneralSettings xrSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            xrSettings.Manager = managerSettings;
            xrSettings.name = "Lynx Settings";

            // Use default Khronos controller
            FeatureHelpers.RefreshFeatures(buildTarget);
            UnityEngine.XR.OpenXR.Features.OpenXRFeature feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(buildTarget, KHRSimpleControllerProfile.featureId);
            if (feature)
            {
                feature.enabled = true;
                Debug.Log($"{feature.name} selected.");
            }

            // Feature set
            OpenXRFeatureSetManager.FeatureSet lynxFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(buildTarget, LynxFeatureSet.featureId);
            if (lynxFeatureSet != null)
            {
                lynxFeatureSet.isEnabled = true;
                lynxFeatureSet.requiredFeatureIds = new string[] { LynxR1Feature.featureId, LynxFeatureSet.xrHandsSubsystemId, LynxFeatureSet.ultraleapFeatureId };
                Debug.Log($"{lynxFeatureSet.name} enabled.");
            }
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(buildTarget);

            XRGeneralSettingsPerBuildTarget xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            xrGeneralSettings.SetSettingsForBuildTarget(buildTarget, xrSettings);

            // Try to enable OpenXR
            XRGeneralSettings xr = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTarget);
            if (xr == null)
            {
                XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
                if(EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings))
                    xr = buildTargetSettings.SettingsForBuildTarget(buildTarget);
            }

            if (xr != null)
            {
                XRManagerSettings xrPlugin = xr.AssignedSettings;
                XRPackageMetadataStore.AssignLoader(xrPlugin, typeof(OpenXRLoader).FullName, buildTarget);
            }
            else
                Debug.LogWarning("OpenXR not enabled.\nYou can enable it manually from XR Plugin Manager under Project settings.");
        }
#endif

        public static void ConfigureProjectSettings()
        {

            // Manage Android part
            LynxBuildSettings.SetupAndroidBuild();

            // For OpenXR, linear color is required
            PlayerSettings.colorSpace = ColorSpace.Linear;

#if LYNX_OPENXR
            // Manage OpenXR part
            ConfigureOpenXR();
#endif
        }

        #endregion

        #region CUSTOM WINDOWS
        /// <summary>
        /// Window warning the user about all the changes on his project.
        /// On validate, it will automatically configure the project settings for the user.
        /// </summary>
        public class ConfigurationWindow : EditorWindow
        {
            void OnGUI()
            {
                GUILayout.Space(20);
                GUILayout.Label("This will automatically configure your project:\n\n" +
                    "- Target Android platform\n" +
                    "\to Android 10 (API Level 29)\n" +
                    "\to ARM64\n" +
                    "\to Enable Multithreaded Rendering\n" +
                    "\to Use OpenGLES graphic API\n" +
                    "\to Set Landscape left\n", EditorStyles.label);


                GUILayout.Space(10);
#if LYNX_OPENXR
                GUILayout.Label("- Configure OpenXR for Android:\n" +
                    "\to Select OpenXR in XR Plugin Management\n" +
                    "\to Use default Khronos controller (to avoid warning)\n" +
                    "\to Select Hand Tracking Subsystem provider\n" +
                    "\to Select Lynx-R1 provider\n" +
                    "\to Select Ultraleap provider\n", EditorStyles.label);
#else
                GUILayout.Label("Cannot configure OpenXR automatically.\nOpenXR is missing.\n", EditorStyles.label);
#endif

                GUILayout.Space(10);
#if ULTRALEAP_TRACKING
                GUILayout.Label("- Configure Ultraleap handtracking settings\n", EditorStyles.label);
#else
                GUILayout.Label("Cannot configure Ultraleap handtracking automatically.\nUltraleap package is missing.\n", EditorStyles.label);
#endif

                if (GUILayout.Button("Validate"))
                {
                    Debug.Log("Configuring project...");
                    ConfigureProjectSettings();
                    this.Close();
                }


                if (GUILayout.Button("Cancel"))
                {
                    this.Close();
                }

            }
        }
#endregion
    }
}