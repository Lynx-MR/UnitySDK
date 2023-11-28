using UnityEngine;

namespace Lynx
{
    public static class LynxSdCard
    {
        public static string path { get; private set; }
        /// <summary>
        /// To Access the external SD card Path in your Lynx. 
        /// You have to add using Lynx and you can access only by calling LynxSdCard.path
        /// </summary>
        static LynxSdCard()
        {
            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.lynx.importlibrary.externalSDcard"))
                {
                    if (pluginClass != null)
                    {
                        path = pluginClass.CallStatic<string>("CallExternalStorageInfo", activityContext);
                    }
                }
            }
        }
    }


}

