using System;
using UnityEngine;
using UnityEngine.Events;

namespace Lynx
{
    public class AndroidComMng : MonoBehaviour
    {
        //PUBLIC
        [HideInInspector] public UnityEvent mAndroidSystemComPlugInInitSucceedEvent;
        [HideInInspector] public UnityEvent mAndroidSystemComPlugInInitFailedEvent;

        [HideInInspector] public CustomUnityIntEvent mAudioVolumeChangeEvent = null;
        [HideInInspector] public CustomUnityIntEvent mBatteryLevelChangeEvent = null;


        //PRIVATE
        private const string LynxAndroidSystemComPlugInName = "com.lynx.lynxandroidsystemcom.LynxAndroidSystemComMng";

        private AndroidJavaClass mAndroidSystemComPlugIn = null;
        private AndroidJavaObject mCurrentActivity = null;
        private AndroidJavaObject mApplicationContext = null;

        //Singleton
        private static AndroidComMng AndroidComMngInstance = null;
        public static AndroidComMng Instance()
        {
            if (!AndroidComMngInstance)
            {
                AndroidComMngInstance = FindObjectOfType(typeof(AndroidComMng)) as AndroidComMng;
                if (!AndroidComMngInstance)
                {
                    Debug.LogError("There needs to be one active AndroidComMng script on a GameObject in your scene.");
                }
            }
            return AndroidComMngInstance;
        }

        public static bool IsInstanceValid() => AndroidComMngInstance != null;


        private void Awake()
        {
            if (mAndroidSystemComPlugInInitSucceedEvent == null)
                mAndroidSystemComPlugInInitSucceedEvent = new UnityEvent();

            if (mAndroidSystemComPlugInInitFailedEvent == null)
                mAndroidSystemComPlugInInitFailedEvent = new UnityEvent();

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidInit();
#endif
        }


        //Android Initialization
        private void AndroidInit()
        {
            // Important : Initialisation of the java plugin :
            InitAndroidJavaPlugin(ref mAndroidSystemComPlugIn, ref mCurrentActivity, ref mApplicationContext, mAndroidSystemComPlugInInitSucceedEvent, mAndroidSystemComPlugInInitFailedEvent);

            // Pass the Unity object name to the plug in to send Message from java to Unity
            mAndroidSystemComPlugIn.CallStatic("setUnityGameObjectName", gameObject.name);

            if (mAudioVolumeChangeEvent == null)
                mAudioVolumeChangeEvent = new CustomUnityIntEvent();

            if (mBatteryLevelChangeEvent == null)
                mBatteryLevelChangeEvent = new CustomUnityIntEvent();

            mAndroidSystemComPlugIn.CallStatic("registerChangesReceivers", mCurrentActivity, mApplicationContext);
        }
        public static void InitAndroidJavaPlugin(ref AndroidJavaClass comPlugin, ref AndroidJavaObject activity, ref AndroidJavaObject context, UnityEvent succeedCallback = null, UnityEvent failedCallback = null)
        {
            try
            {
                //Set Android  plugin
                var plugin = new AndroidJavaClass(LynxAndroidSystemComPlugInName);
                if (plugin != null)
                {
                    comPlugin = plugin;
                    string libJarVersion = comPlugin.CallStatic<string>("getVersion");
                    Debug.Log("LynxAndroidSystemComMng was correctly initialized and its version is " + libJarVersion);
                    Debug.Log(" ");
                }
                else
                {
                    Debug.LogError("mAndroidSystemComPlugIn is NULL");
                    return;
                }

                //Set unityPlayer
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                if (unityPlayer == null)
                {
                    Debug.LogError("unityPlayer is NULL");
                    return;
                }

                //Set CurrentActivity
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null)
                {
                    Debug.LogError("CurrentActivity is NULL");
                    return;
                }

                //Set ApplicationContext
                context = activity.Call<AndroidJavaObject>("getApplicationContext");
                if (context == null)
                {
                    Debug.LogError("ApplicationContext is NULL");
                    return;
                }

                //If all Sets succeeded, invoke Success Event
                succeedCallback?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                Debug.Log("----------- Init AndroidSystemComPlugIn FAILED");

                // tell the user that the plug in init fails : 
                failedCallback?.Invoke();
            }
        }

        //CALLBACKS sent from Java Plugin :
        public void AudioVolumeChange(string volume)
        {
            // Volume sent varies from 0 to 15. 
            //Debug.Log("------------------- VolumeChange(string volume) received in OSComMng = " + volume);
            int iVolume = 0;
            int.TryParse(volume, out iVolume);
            mAudioVolumeChangeEvent.Invoke(iVolume);
        }
        public void BatteryLevelChange(string batteryLevel)
        {
            // Battery level varies from 0 to 100. it's a percentage 
            //Debug.Log("------------------- BatteryLevelChange(string batteryLevel) received in OSComMng = " + batteryLevel);
            int iBatteryLevel = 0;
            int.TryParse(batteryLevel, out iBatteryLevel);
            mBatteryLevelChangeEvent.Invoke(iBatteryLevel);
        }


        //Current App Data Getters
        public string GetAppName()
        {
            //Debug.Log("----------- GetAppName()");
            string appName = Application.productName;
            return appName;
        }
        public SByte[] GetAppIconBytes()
        {
            //Debug.Log("----------- GetAppName()");
            string appPackageName = Application.identifier;

            int flag = new AndroidJavaClass("android.content.pm.PackageManager").GetStatic<int>("GET_META_DATA");
            AndroidJavaObject pm = mCurrentActivity.Call<AndroidJavaObject>("getPackageManager");

            SByte[] decodedBytes = mAndroidSystemComPlugIn.CallStatic<SByte[]>("GetIcon", pm, appPackageName);

            if (decodedBytes == null) // Cedric : it could happens
            {
                Debug.LogWarning("------------- No App icon for package name : " + appPackageName);
            }

            return decodedBytes;
        }
        public Texture2D GetAppIconTexture()
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
        public int GetBatteryLevel()
        {
            if (mAndroidSystemComPlugIn == null)
            {
                return 0;
            }

            return mAndroidSystemComPlugIn.CallStatic<int>("getBatteryPercentage", mApplicationContext);
        }

        //Device_Audio_Mng
        public int GetAudioVolume()
        {
            return mAndroidSystemComPlugIn.CallStatic<int>("getAudioVolume", mApplicationContext);
        }
        public int GetMaxAudioVolume()
        {
            return mAndroidSystemComPlugIn.CallStatic<int>("getMaxAudioVolume", mApplicationContext);
        }
        public void SetAudioVolume(int volume) // volume is 0 to 15. (integer)
        {
            if (volume < 0 || volume > 15)
            {
                Debug.LogWarning("Try to set an audio volume not between 0 to 15, setAudioVolume will be not called");
                return;
            }

            mAndroidSystemComPlugIn.CallStatic("setAudioVolume", mApplicationContext, volume);
        }
        public void SetMicrophoneMute(bool mute)
        {
            mAndroidSystemComPlugIn.CallStatic("setMicrophoneMute", mApplicationContext, mute);
        }
        public bool isMicrophoneMute()
        {
            return mAndroidSystemComPlugIn.CallStatic<bool>("isMicrophoneMute", mApplicationContext);
        }

    }
}