/**
 * @file AndroidVersionChecker.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Helper to compare versions, expecially for OS updates.
 */

using System.Text.RegularExpressions;
using UnityEngine;

namespace Lynx
{
    public static class AndroidVersionChecker
    {
        private static AndroidJavaClass androidPlugin = null;

        static AndroidVersionChecker()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject androidActivity = null;
            AndroidJavaObject androidContext = null;
            AndroidComMng.InitAndroidJavaPlugin(ref androidPlugin, ref androidActivity, ref androidContext);
#endif
        }

        /// <summary>
        /// Compare given version with current installed version.
        /// </summary>
        /// <param name="version">Given version to compare with system version.</param>
        /// <returns>1: given version is higher than system version. 0: same version than system build. -1: given version is lower than system version. -2: error with version</returns>
        public static int CompareVersion(string version)
        {
            return CompareVersion(version, GetLynxAndroidSystemVersion());
        }

        /// <summary>
        /// Compare two versions (format xx.yy.zz).
        /// </summary>
        /// <param name="version1">First version to compare.</param>
        /// <param name="version2">Second version to compare.</param>
        /// <returns>1: first version is higher. 0: same versions. -1: first version is lower. -2: error with version</returns>
        public static int CompareVersion(string version1, string version2)
        {
            // Prepare regular expresion to extract version
            Regex rx = new Regex("[0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Extract first version data
            MatchCollection version1Matches = rx.Matches(version1);
            if (version1Matches.Count != 3)
            {
                Debug.LogError($"Version {version1} is invalid");
                return -2;
            }
            int[] version1Values = new int[] { int.Parse(version1Matches[0].Value), int.Parse(version1Matches[1].Value), int.Parse(version1Matches[2].Value) };


            // Extract second version data
            MatchCollection version2Matches = rx.Matches(version2);
            if (version2Matches.Count != 3)
            {
                Debug.LogError($"Version {version2} is invalid");
                return -3;
            }
            int[] version2Values = new int[] { int.Parse(version2Matches[0].Value), int.Parse(version2Matches[1].Value), int.Parse(version2Matches[2].Value) };

            return CheckVersion(version1Values, version2Values, 0, 2);
        }

        /// <summary>
        /// Recursive to do comparaison on each digit of the version
        /// </summary>
        /// <param name="version1">Values as int array for first version to compare</param>
        /// <param name="version2">Values as int array for second version to compare</param>
        /// <param name="idx">Current idx (start at 0)</param>
        /// <param name="maxIdx">Idx limitation.</param>
        /// <returns>1: first version is higher. 0: same versions. -1: first version is lower. -2: error with version</returns>
        public static int CheckVersion(int[] version1, int[] version2, int idx, int maxIdx)
        {
            if (version1[idx] == version2[idx])
            {
                if (idx == maxIdx)
                    return 0;
                else
                    return CheckVersion(version1, version2, ++idx, maxIdx);
            }
            else
                return (version1[idx] < version2[idx]) ? -1 : 1;
        }


        /// <summary>
        /// Get current OS information.
        /// </summary>
        /// <returns>String with all device and system info.</returns>
        public static string GetDeviceAndSystemInfo()
        {
            string ret = "";
            if (androidPlugin != null)
                ret = androidPlugin.CallStatic<string>("getDeviceAndSystemInfo");
            return ret;
        }

        /// <summary>
        /// Get current OS version.
        /// </summary>
        /// <returns>String representation of the current version.</returns>
        public static string GetLynxAndroidSystemVersion()
        {
            string ret = "1.0.0";

            if (androidPlugin != null)
                ret = androidPlugin.CallStatic<string>("getLynxAndroidSystemVersion");

            return ret;
        }
    }
}