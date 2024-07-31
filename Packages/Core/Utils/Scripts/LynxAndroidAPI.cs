/**
 * @file LynxAndroidAPI.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Direct access to android managers. Provide an API for some android features available in Java.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lynx
{
    public class LynxAndroidAPI
    {
        /// <summary>
        /// Application package information for app name, package name and icon information.
        /// </summary>
        public struct AndroidPackageInfo
        {
            public string name;
            public string packageName;
            public int iconWidth;
            public int iconHeight;
            public byte[] icon;
            public bool HasIcon;
            public long lastUpdate;
        }

        /// <summary>
        /// Retrieve the apps installed on the system.
        /// This process could take a while to proceed. To avoid blocking the UI, we recommend to call this function on a different thread.
        /// Please refer to AndroidJNI.AttachCurrentThread(), for JNI call from Unity using the JVM correctly.
        /// </summary>
        /// <param name="filter">Only take pacakge having this filter in the name (null by default to take all applications info)</param>
        /// <returns>List of the app containing the name, the package info and icon details.</returns>
        public static List<AndroidPackageInfo> GetInstalledApps(string filter = null)
        {
            List<AndroidPackageInfo> res = new List<AndroidPackageInfo>();

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            AndroidJavaClass packageManagerClass = new AndroidJavaClass("android.content.pm.PackageManager");
            int flag = packageManagerClass.GetStatic<int>("GET_META_DATA");

            AndroidJavaObject pkgManager = context.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject packagesList = pkgManager.Call<AndroidJavaObject>("getInstalledApplications", flag);
            if (packagesList != null)
            {
                int count = packagesList.Call<int>("size");


                // Hold one instance to class objects
                AndroidJavaClass bitmapClass = new AndroidJavaClass("android.graphics.Bitmap"); // For cast method
                AndroidJavaObject pngFormat = (new AndroidJavaClass("android.graphics.Bitmap$CompressFormat")).GetStatic<AndroidJavaObject>("PNG");


                for (int i = 0; i < count; ++i)
                {
                    AndroidJavaObject obj = packagesList.Call<AndroidJavaObject>("get", i);
                    AndroidPackageInfo androidPackageInfo = new AndroidPackageInfo();

                    // Package name
                    //androidPackageInfo.packageName = obj.Call<string>("toString"); //Package name: ApplicationInfo{92a79a0 com.android.bluetoothmidiservice}
                    androidPackageInfo.packageName = obj.Get<string>("packageName"); //Package name: com.android.bluetoothmidiservice

                    // Package last update
                    try
                    {
                        AndroidJavaObject packageInfo = pkgManager.Call<AndroidJavaObject>("getPackageInfo", androidPackageInfo.packageName, 0);
                        androidPackageInfo.lastUpdate = packageInfo.Get<long>("lastUpdateTime");
                    }
                    catch(AndroidJavaException e)
                    {
                        Debug.LogError("Error when obtaining package information: " + e.Message);
                    }
                    

                // Name
                androidPackageInfo.name = pkgManager.Call<string>("getApplicationLabel", obj);
                    if (filter != null && !androidPackageInfo.packageName.Contains(filter))
                        continue;

                    try
                    {
                        // Icon
                        AndroidJavaObject iconDrawable = pkgManager.Call<AndroidJavaObject>("getApplicationIcon", obj);

                        if (iconDrawable.Call<AndroidJavaObject>("getClass").Call<string>("getName").Equals("android.graphics.drawable.BitmapDrawable") || iconDrawable.Call<AndroidJavaObject>("getClass").Call<string>("getName").Equals("android.graphics.drawable.AdaptiveIconDrawable"))
                        {
                            // Get drawable icon
                            AndroidJavaObject drawable = pkgManager.Call<AndroidJavaObject>("getApplicationIcon", obj);

                            // Get dimension
                            AndroidJavaObject bitmap = bitmapClass.CallStatic<AndroidJavaObject>("createBitmap", drawable.Call<int>("getIntrinsicWidth"), drawable.Call<int>("getIntrinsicHeight"), (new AndroidJavaClass("android.graphics.Bitmap$Config")).GetStatic<AndroidJavaObject>("ARGB_8888"));

                            // Create Canvas and bind it to the bitmap
                            AndroidJavaObject canvas = new AndroidJavaObject("android.graphics.Canvas", bitmap);
                            int width = canvas.Call<int>("getWidth");
                            int height = canvas.Call<int>("getHeight");

                            drawable.Call("setBounds", 0, 0, width, height);
                            drawable.Call("draw", canvas);

                            //// Retrieve buffer
                            AndroidJavaObject byteArraOutputStream = new AndroidJavaObject("java.io.ByteArrayOutputStream");
                            bitmap.Call<bool>("compress", pngFormat, 100, byteArraOutputStream);

                            androidPackageInfo.icon = (byte[])(Array)byteArraOutputStream.Call<sbyte[]>("toByteArray");
                            androidPackageInfo.iconWidth = width;
                            androidPackageInfo.iconHeight = height;
                            androidPackageInfo.HasIcon = true;
                        }
                        else
                            androidPackageInfo.HasIcon = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"\t{ex.Message}\t{androidPackageInfo.name}");
                    }

                    res.Add(androidPackageInfo);
                }
            }

            return res;
        }

        /// <summary>
        /// Retrieve a most complete system informations list
        /// </summary>
        /// <returns>A string list of the system information (complete build name, build version, model, brand, release version, os version, incremental version, sdk version, device name, fingerprint)</returns>
        public static List<string> GetFullSystemInfo()
        {
            List<string> res = new List<string>();

            AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build");
            AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
            res.Add(buildClass.GetStatic<string>("MODEL"));
            res.Add(buildClass.GetStatic<string>("BRAND"));
            res.Add(versionClass.GetStatic<string>("RELEASE"));
            res.Add(versionClass.GetStatic<string>("BASE_OS"));
            res.Add(versionClass.GetStatic<string>("INCREMENTAL"));
            res.Add($"{versionClass.GetStatic<int>("SDK_INT")}");
            res.Add(buildClass.GetStatic<string>("DEVICE"));
            res.Add(buildClass.GetStatic<string>("FINGERPRINT"));
            res.Add(buildClass.GetStatic<string>("DISPLAY"));

            return res;
        }

        /// <summary>
        /// Retrieve installed system version.
        /// </summary>
        /// <returns>System version format x.y.z (ex: 1.1.2)</returns>
        public static string GetSystemVersion()
        {
            string res = "1.0.0";

            string fullFingerPrint = (new AndroidJavaClass("android.os.Build")).GetStatic<string>("FINGERPRINT");


            string[] paths = fullFingerPrint.Split('/');
            if (paths.Length < 4)
                Debug.LogError("Error: version undefined");
            else
                res = paths[3].Remove(0, 1);

            return res;
        }

        /// <summary>
        /// Call this function to launch an application by package name.
        /// </summary>
        /// <param name="packageName">Package name, ex: com.Lynx.Onboarding.</param>
        public static void LaunchApp(string packageName)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager");
            
            AndroidJavaObject launchIntent = null;

            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Error when launching package: " + e.Message);
                return;
            }

            currentActivity.Call("startActivity", launchIntent);
        }
    }
}