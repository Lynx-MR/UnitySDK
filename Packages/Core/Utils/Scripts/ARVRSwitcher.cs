/**
 * @file ARVRSwitcher.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Define AR or VR mode using a config file. A config file will be created if missing based on current mode. Inform user of the current mode if needed.
 */
using System.IO;
using UnityEngine;

namespace Lynx
{
    public class ARVRSwitcher : MonoBehaviour
    {

        #region CONSTANTES
        private const string CONFIG_NAME = "AR_config.txt";
        #endregion

        #region VARIABLES
        public string ConfigPath { get; private set; }
        #endregion

        #region UNITY API
        private void Awake()
        {
            Instance = this; // Singleton

#if UNITY_EDITOR
            ConfigPath = Path.Combine(Application.dataPath, "..", CONFIG_NAME);
#else
            ConfigPath = Path.Combine(Application.persistentDataPath, CONFIG_NAME);
#endif
            Debug.Log($"Search config file at {ConfigPath}");
            if (File.Exists(ConfigPath))
            {
                // Retrieve config file to define if should be AR or VR mode enabled.
                //TextAsset txt = (TextAsset)Resources.Load(CONFIG_NAME, typeof(TextAsset));
                string configAR = File.ReadAllText(ConfigPath);
                IsAR = bool.Parse(configAR);
                Debug.Log(IsAR + " from file.");
                

                // Enable or disable AR
                if(IsAR == false)
                {
                    EnablePassThru(false);
                }
                else // Null or false
                {
                    EnablePassThru();

                }
            }
            else // Create config file if doest not exist (based on current configuration)
            {
                IsAR = IsPassThru();
                WriteConfig((bool)IsAR);
                Debug.Log(IsAR + " from settings");
            }
        }
        #endregion

        #region METHODS
        /// <summary>
        /// Define if current mode
        /// </summary>
        public bool? IsAR { get; private set; } = null;

        /// <summary>
        /// Write into the config file the mode to use when starting.
        /// </summary>
        /// <param name="isAR">True: Pass Through. False: VR mode.</param>
        public void WriteConfig(bool isAR)
        {
            File.WriteAllText(ConfigPath, isAR.ToString());
        }

        public void EnablePassThru(bool enable = true)
        {
            throw new System.Exception("Enabling Video Pass Thru is not implemented for OpenXR yet");

            /* PREVIOUSLY  
             *      m_svrManager.settings.cameraPassThruVideo = ENABLED;
             
                    // Disable cameras
                    m_svrManager.leftCamera.clearFlags = CameraClearFlags.SolidColor;
                    m_svrManager.leftCamera.backgroundColor = new Color(.0f, .0f, .0f, .0f);

                    m_svrManager.rightCamera.clearFlags = CameraClearFlags.SolidColor;
                    m_svrManager.rightCamera.backgroundColor = new Color(.0f, .0f, .0f, .0f);
            */
        }

        public bool IsPassThru()
        {
            Debug.LogWarning("Check Video Pass Thru is not implemented for OpenXR yet");
            return false;
        }
        #endregion

        #region SINGLETON
        public static ARVRSwitcher Instance { get; private set; }
        protected ARVRSwitcher() { }
        #endregion
    }
}