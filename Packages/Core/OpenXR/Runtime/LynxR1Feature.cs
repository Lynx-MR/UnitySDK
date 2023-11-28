/**
 * @file LynxR1Feature.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Create an OpenXR Feature for Lynx R1.
 */

using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR.NativeTypes;
using System.Collections.Generic; 

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif


namespace Lynx.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        UiName = "Lynx-R1",
        Company = "Lynx",
        Desc = "OpenXR support for Lynx-R1 headset.",
        DocumentationLink = "",
        Version = "1.0.0",
        FeatureId = featureId
    )]
#endif
    public class LynxR1Feature : OpenXRFeature
    {
        public const string featureId = "com.lynx.openxr.feature";

#if UNITY_EDITOR
        public static LynxR1Feature Instance
        {
            get  => throw new System.Exception("Cannot use Lynx API in Editor.\n\tNote: You can use \"#if !UNITY_EDITOR\" in your code to avoid device dependent code.");
            private set { }
        }
#else
        public static LynxR1Feature Instance { get; private set; } = null;
#endif

        #region INSPECTOR
        [Tooltip("Check to start application in AR mode. Otherwise, the application starts in VR (can be change at runtime using LynxAPI).")]
        public bool startInAR = true;
        [Tooltip("Enable this feature to mix use of the background (eg: skybox) in VR and hide it in AR")]
        public bool DisableBackgroundInAR = true;

        [Tooltip("Layer index for object to display in VR only (hidden in AR)")]
        public int onlyVRLayer = 10;
        [Tooltip("Layer index for object to display in AR only (hidden in VR)")]
        public int onlyARLayer = 11;
        [Tooltip("Tag to use on root objects to stay visible in Full AR (handtracking, GameManager, ...).")]
        public string VisibleInFullARTag= "FullARObject";
        #endregion

        #region VARIABLES
        // Default clear flags for the camera
        private CameraClearFlags m_startFlags = CameraClearFlags.Nothing;

        // Default camera background color
        private Color m_startBackgroundColor = Color.black;

        // AR mode requested from runtime.
        private bool m_isARModeRequested = false;

        // Store list of all hidden object when using AR only
        private List<GameObject> m_hiddenObjects = new List<GameObject>();
        #endregion

        #region PROPERTIES
        // Current status of AR only
        public bool IsAROnly { get; private set; }

        public delegate void ARVRSwitchEvent(bool isAR);
        public ARVRSwitchEvent onARVRChanged = null;
        #endregion

        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);
            Instance = this;

            if (startInAR)
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);
                onARVRChanged?.Invoke(true);
                m_isARModeRequested = true;
            }
            else
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.Opaque);
                onARVRChanged?.Invoke(false);
                m_isARModeRequested = false;
            }
        }

        protected override void OnEnvironmentBlendModeChange(XrEnvironmentBlendMode xrEnvironmentBlendMode)
        {
            base.OnEnvironmentBlendModeChange(xrEnvironmentBlendMode);

            // Unity OpenXR Plugin looses XrEnvironmentBlendMode when app move from background to foreground
            if (xrEnvironmentBlendMode == XrEnvironmentBlendMode.Opaque && m_isARModeRequested == true)
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);
            }
            else if (xrEnvironmentBlendMode == XrEnvironmentBlendMode.AlphaBlend && m_isARModeRequested == false)
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.Opaque);
            }
        }

        /// <summary>
        /// Used once the splashscreen has finished and the scene is correctly loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void OnSceneLoaded()
        {
#if UNITY_EDITOR
            Debug.LogWarning("AR not enabled in Editor");
#else
            Instance.m_startFlags = Camera.main.clearFlags;
            Instance.m_startBackgroundColor = Camera.main.backgroundColor;

            if (Instance.IsAR())
                Instance.SetAR();
            else
                Instance.SetVR();
#endif
        }

        /// <summary>
        /// Retrieve current Video See Through status.
        /// </summary>
        /// <returns>True: the current mode is VR. False: the current mode is AR. </returns>
        public bool IsVR()
        {
            return GetEnvironmentBlendMode() == XrEnvironmentBlendMode.Opaque;
        }

        /// <summary>
        /// Retrieve current Video See Through status.
        /// </summary>
        /// <returns>True: the current mode is AR. False: the current mode is VR. </returns>
        public bool IsAR()
        {
            return !IsVR();
        }

        /// <summary>
        /// Set video see through mode.
        /// </summary>
        /// <param name="isAR">True (default) to set video see through mode. False to set VR mode.</param>
        public void SetAR(bool isAR = true)
        {
            if(isAR)
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);
                Camera.main.cullingMask &= ~(1 << onlyVRLayer);
                Camera.main.cullingMask |= (1 << onlyARLayer);

                // Remove background for AR
                if (DisableBackgroundInAR)
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Color bgColor = Camera.main.backgroundColor;
                    bgColor.a = 0.0f;
                    Camera.main.backgroundColor = bgColor;
                }
                m_isARModeRequested = true;
            }
            else
            {
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.Opaque);
                Camera.main.cullingMask |= (1 << onlyVRLayer);
                Camera.main.cullingMask &= ~(1 << onlyARLayer);

                // Restore background for VR
                if (DisableBackgroundInAR)
                {
                    Camera.main.clearFlags = m_startFlags;
                    Camera.main.backgroundColor = m_startBackgroundColor;
                }
                m_isARModeRequested = false;
            }

            onARVRChanged?.Invoke(isAR);
        }

        /// <summary>
        /// Set VR mode.
        /// </summary>
        /// <param name="isVR">True (default) to set VR mode. False to set video see through mode.</param>
        public void SetVR(bool isVR = true)
        {
            SetAR(!isVR);
        }

        /// <summary>
        /// Make all active object disappear, except for OpenXR specific objects
        /// </summary>
        /// <param name="enableAROnly"></param>
        public void SetAROnly(bool enableAROnly = true)
        {
            if (enableAROnly)
            {
                // Currently, it's only use root objects for a quick response. But can be improved for all objects in the scene if needed (slow downs the event).
                GameObject[] tmpList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                for (int i = 0, count = tmpList.Length; i < count; ++i)
                {
                    if (tmpList[i].activeInHierarchy && !tmpList[i].GetComponentInChildren<Camera>() && !tmpList[i].CompareTag(VisibleInFullARTag))
                    {
                        tmpList[i].SetActive(false);
                        m_hiddenObjects.Add(tmpList[i]);
                    }
                }

                
            }
            else
            {
                // Reactive each object and remove it from the hidden objects list
                while(m_hiddenObjects.Count > 0)
                {
                    m_hiddenObjects[0].SetActive(true);
                    m_hiddenObjects.RemoveAt(0);
                }
            }

            IsAROnly = enableAROnly;

            SetAR(enableAROnly);
        }
    }
}