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
using UnityEngine.XR.OpenXR.Features;

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
        [MenuItem("Lynx/Settings/Configure Project Settings", false, 100)]
        public static void ConfigureProject()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(ConfigurationWindow));
            Rect r = window.position;
            r.position = new Vector2(150.0f, 150.0f);
            r.width = 400.0f;
            r.height = 420.0f;
            window.position = r;
        }
        #endregion

        #region METHODS
        public static void ClearOpenXRSettings(BuildTargetGroup buildTarget)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
            OpenXRFeature[] featuresToRemove = settings.GetFeatures();
            foreach (OpenXRFeature f in featuresToRemove)
            {
                if (f.enabled)
                    f.enabled = false;
            }
        }

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



            /********** OPENXR **********/
            // Clear existing profiles and providers
            ClearOpenXRSettings(buildTarget);

            // Use Hand Interaction Controller
            FeatureHelpers.RefreshFeatures(buildTarget);

            // Select Hand Interaction Controller Profile
            OpenXRFeature[] features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(buildTarget, new string[] { HandInteractionProfile.featureId, LynxFeatureSet.xrHandsSubsystemId, LynxFeatureSet.xrHandtrackingAim });
            foreach(OpenXRFeature feature in features)
            {
                if (feature)
                {
                    feature.enabled = true;
                    Debug.Log($"{feature.name} selected.");
                }
            }
            

            /********** PROVIDERS **********/
            // Feature set
            OpenXRFeatureSetManager.FeatureSet lynxFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(buildTarget, LynxFeatureSet.featureId);
            if (lynxFeatureSet != null)
            {
                lynxFeatureSet.isEnabled = true;
                //lynxFeatureSet.requiredFeatureIds = new string[] { LynxR1Feature.featureId, LynxFeatureSet.xrHandsSubsystemId, LynxFeatureSet.xrHandtrackingAim };
                //lynxFeatureSet.featureIds = new string[] { LynxR1Feature.featureId, LynxFeatureSet.xrHandsSubsystemId, LynxFeatureSet.xrHandtrackingAim };
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
            bool StarterAssets = false;
            bool HandInteractionDemo = false;
            bool HandVisualizer = false;
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
                    "\to Use Hand Interaction Profile\n" +
                    "\to Select Hand Tracking Subsystem provider\n" +
                    "\to Select Meta Hand Tracking Aim provider\n" +
                    "\to Select Lynx-R1 provider\n", EditorStyles.label);
#else
                GUILayout.Label("Cannot configure OpenXR automatically.\nOpenXR is missing.\n", EditorStyles.label);
#endif

                GUILayout.Space(10);
                GUILayout.Label("- Import samples dependency : ");
                GUILayout.BeginHorizontal();
                GUILayout.Space(45);
                StarterAssets = GUILayout.Toggle(StarterAssets, " XRI - Starter Assets");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(45);
                HandInteractionDemo = GUILayout.Toggle(HandInteractionDemo, " XRI - Hand Interaction Demo");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(45);
                HandVisualizer = GUILayout.Toggle(HandVisualizer, " XR Hands - Hand Visualizer");
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                if (GUILayout.Button("Validate"))
                {
                    Debug.Log("Configuring project...");
                    ConfigureProjectSettings();
                    if (StarterAssets)
                        SampleImporter.ImportStarterAssets();
                    if (HandInteractionDemo)
                        SampleImporter.ImportHandsInteractionDemo();
                    if (HandVisualizer)
                        SampleImporter.ImportHandVizualiser();
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