#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif

using System;
using UnityEngine;

namespace Lynx
{
    public static class AndroidComMng
    {

        //PUBLIC
        public delegate void AudioVolumeChangeEvent(int audioVolume);
        public static AudioVolumeChangeEvent OnAudioVolumeChange = null;

        public delegate void BatteryLevelChangeEvent(int audioVolume);
        public static BatteryLevelChangeEvent OnABatteryLevelChange = null;

        public static Action OnAndroidSystemComPlugInInitSucceedEvent = null;
        public static Action OnAndroidSystemComPlugInInitFailedEvent = null;

        //PRIVATE
        private const string m_LynxAndroidSystemComPlugInName = "com.lynx.lynxandroidsystemcom.LynxAndroidSystemComMng";

        private static AndroidJavaClass m_AndroidSystemComPlugIn = null;
        private static AndroidJavaObject m_CurrentActivity = null;
        private static AndroidJavaObject m_ApplicationContext = null;

        static AndroidComMng()
        {
#if !LYNX_HOME_LAUNCHER
            Debug.Log("------------ AndroidComMng() static constructor called -----------");
           // Static initialization code here
           Init();
#else
            Debug.Log("------------ AndroidComMng() : No init() called because there is a specific init done in the LYNX HOME LAUNCHER");
#endif                          
        }


        public static bool Init()
        {
#if LYNX

            //Application.SetStackTraceLogType(LogType.Log , StackTraceLogType.None);
            //Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            return AndroidInit();
#else
            return false;
#endif
        }


        //Android Initialization
        private static bool AndroidInit()
        {
            if (!InitAndroidJavaPlugin(ref m_AndroidSystemComPlugIn, ref m_CurrentActivity, ref m_ApplicationContext, OnAndroidSystemComPlugInInitSucceedEvent, OnAndroidSystemComPlugInInitFailedEvent))
                return false;

            // Initialize : Call all changes receiver (Battery,volume, network, package installation) and register callback
            // for the communication from Java plugin to Unity
            m_AndroidSystemComPlugIn.CallStatic("Initialize", m_ApplicationContext, new AndroidPluginCallback());

            return true;
        }

        public static bool InitAndroidJavaPlugin(ref AndroidJavaClass comPlugin, ref AndroidJavaObject activity, ref AndroidJavaObject context, Action succeedCallback = null, Action failedCallback = null)
        {
            try
            {
                //Set Android  plugin
                var plugin = new AndroidJavaClass(m_LynxAndroidSystemComPlugInName);

                if (plugin != null)
                {
                    comPlugin = plugin;
                }
                else
                {
                    Debug.LogError("Android plugin is NULL");
                    return false;
                }

                //Set unityPlayer
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                if (unityPlayer == null)
                {
                    Debug.LogError("unityPlayer is NULL");
                    return false;
                }

                //Set CurrentActivity
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null)
                {
                    Debug.LogError("CurrentActivity is NULL");
                    return false;
                }

                //Set ApplicationContext
                context = activity.Call<AndroidJavaObject>("getApplicationContext");
                if (context == null)
                {
                    Debug.LogError("ApplicationContext is NULL");
                    return false;
                }

                string libJarVersion = comPlugin.CallStatic<string>("getVersion");
                Debug.Log("LynxAndroidSystemComMng was correctly initialized and its version is " + libJarVersion);
                Debug.Log(" ");

                //If all Sets succeeded, invoke Success Event
                succeedCallback?.Invoke();

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.Log("----------- Init Android System Com Plug-In FAILED");

                // tell the user that the plug in init fails : 
                failedCallback?.Invoke();

                return false;
            }
        }

        //CALLBACKS sent from Java Plugin :
        public static void AudioVolumeChange(string volume)
        {
            // Volume sent varies from 0 to 15. 
            //Debug.Log("------------------- VolumeChange(string volume) received in OSComMng = " + volume);
            int iVolume = 0;
            int.TryParse(volume, out iVolume);
            OnAudioVolumeChange?.Invoke(iVolume);
        }
        public static void BatteryLevelChange(string batteryLevel)
        {
            // Battery level varies from 0 to 100. it's a percentage 
            //Debug.Log("------------------- BatteryLevelChange(string batteryLevel) received in OSComMng = " + batteryLevel);
            int iBatteryLevel = 0;
            int.TryParse(batteryLevel, out iBatteryLevel);
            OnABatteryLevelChange?.Invoke(iBatteryLevel);
        }


        //Current App Data Getters
        public static string GetAppName()
        {
            //Debug.Log("----------- GetAppName()");
            string appName = Application.productName;
            return appName;
        }
        public static SByte[] GetAppIconBytes()
        {
            //Debug.Log("----------- GetAppName()");
            string appPackageName = Application.identifier;

            int flag = new AndroidJavaClass("android.content.pm.PackageManager").GetStatic<int>("GET_META_DATA");
            AndroidJavaObject pm = m_CurrentActivity.Call<AndroidJavaObject>("getPackageManager");

            SByte[] decodedBytes = m_AndroidSystemComPlugIn.CallStatic<SByte[]>("GetIcon", pm, appPackageName);

            if (decodedBytes == null) // Cedric : it could happens
            {
                Debug.LogWarning("------------- No App icon for package name : " + appPackageName);
            }

            return decodedBytes;
        }
        public static Texture2D GetAppIconTexture()
        {
            SByte[] decodedBytes = GetAppIconBytes();
            if (decodedBytes == null) return null;

            Texture2D texture2D = null;
            byte[] data;

            texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            data = Array.ConvertAll(decodedBytes, (a) => (byte)a);
            texture2D.LoadImage(data);
            return texture2D;
        }

        //Battery_Mng
        public static int GetBatteryLevel()
        {
            if (m_AndroidSystemComPlugIn == null)
            {
                return 0;
            }

            return m_AndroidSystemComPlugIn.CallStatic<int>("getBatteryPercentage", m_ApplicationContext);
        }

        public static bool isBatteryCharging()
        {
            if (m_AndroidSystemComPlugIn == null)
            {
                return false;
            }

            if (m_AndroidSystemComPlugIn.CallStatic<int>("isBatteryCharging", m_ApplicationContext) > 0)
            {
                return true;
            }

            return false;
        }

        //Device_Audio_Mng
        public static int GetAudioVolume()
        {
            return m_AndroidSystemComPlugIn.CallStatic<int>("getAudioVolume", m_ApplicationContext);
        }
        public static int GetMaxAudioVolume()
        {
            return m_AndroidSystemComPlugIn.CallStatic<int>("getMaxAudioVolume", m_ApplicationContext);
        }
        public static void SetAudioVolume(int volume) // volume is 0 to 15. (integer)
        {
            if (volume < 0 || volume > 15)
            {
                Debug.LogWarning("Try to set an audio volume not between 0 to 15, setAudioVolume will be not called");
                return;
            }

            m_AndroidSystemComPlugIn.CallStatic("setAudioVolume", m_ApplicationContext, volume);
        }
        public static void SetMicrophoneMute(bool mute)
        {
            m_AndroidSystemComPlugIn.CallStatic("setMicrophoneMute", m_ApplicationContext, mute);
        }
        public static bool isMicrophoneMute()
        {
            return m_AndroidSystemComPlugIn.CallStatic<bool>("isMicrophoneMute", m_ApplicationContext);
        }


        private static void DispatchAndroidJavaPlugInMessage(string messageType, string messageArg)
        {
            // Messages from Java plugin are NOT received in the Unity Main Thread
            // and most of the actions linked to this messages changes UI and neead to be execute in 
            // the Unity Main Thread. So we need to call it from a particular object that executes it 
            // in the Unity Main Thread :
            switch (messageType)
            {
                case "BatteryLevelChange":
                        BatteryLevelChange(messageArg);
                    break;

                case "AudioVolumeChange":
                        AudioVolumeChange(messageArg);

                    break;
                default:
                    Debug.LogError("no valid messageType in DispatchAndroidJavaPlugInMessage");
                    break;
            }
        }

        class AndroidPluginCallback : AndroidJavaProxy
        {
            public AndroidPluginCallback() : base("com.lynx.lynxandroidsystemcom.LynxAndroidSystemComCallback") { }

            public void onMessage(string messageType, string messageArg)
            {
                DispatchAndroidJavaPlugInMessage(messageType, messageArg);
            }
        }
    }
}