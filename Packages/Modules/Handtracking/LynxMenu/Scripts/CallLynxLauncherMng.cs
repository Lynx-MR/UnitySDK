using System.Collections;
using UnityEngine;

namespace Lynx
{
    public class CallLynxLauncherMng : MonoBehaviour
    {
        public void BackToLauncher()
        {
            if (LynxAPI.IsAR())
                LynxAPI.SetVR();

            StartCoroutine(BackToLauncherInCoroutine());



            /*
            bool fail = false;

            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = null;

            string lynxLauncherPackageName = "com.lynx.scenehome";

            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", lynxLauncherPackageName);
            }
            catch (System.Exception)
            {
                fail = true;
            }

            if (fail)
            {  //open app in store
                Application.OpenURL("https://lynx-r.com/");
            }
            else //open the app
                ca.Call("startActivity", launchIntent);


            Debug.Log("@@@@@@@@@@@@@@  Before finishAndRemoveTask In Lynx SDK   @@@@@@@@@@@@@@ ");
            ca.Call("finishAndRemoveTask");

            up.Dispose();
            ca.Dispose();
            packageManager.Dispose();
            launchIntent.Dispose();

            // is it useful ?
            //Debug.Log("@@@@@@@@@@@@@@ Before Application quit called @@@@@@@@@@@@@@ ");
            //Application.Quit();
            */

        }


        IEnumerator BackToLauncherInCoroutine()
        {
            yield return new WaitForSecondsRealtime(0.2f);

            Debug.Log("@@@@@@@@@@@@@@  BackToLauncherInCoroutine   @@@@@@@@@@@@@@ ");

            bool fail = false;

            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = null;

            string lynxLauncherPackageName = "com.lynx.scenehome";

            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", lynxLauncherPackageName);
            }
            catch (System.Exception)
            {
                fail = true;
            }

            if (fail)
            {  //open app in store
                Application.OpenURL("https://lynx-r.com/");
            }
            else //open the app
                ca.Call("startActivity", launchIntent);


            Debug.Log("@@@@@@@@@@@@@@  Before finishAndRemoveTask In Lynx SDK   @@@@@@@@@@@@@@ ");
            ca.Call("finishAndRemoveTask");

            up.Dispose();
            ca.Dispose();
            packageManager.Dispose();
            launchIntent.Dispose();

            // is it useful ?
            //Debug.Log("@@@@@@@@@@@@@@ Before Application quit called @@@@@@@@@@@@@@ ");
            //Application.Quit();



        }
    }
}