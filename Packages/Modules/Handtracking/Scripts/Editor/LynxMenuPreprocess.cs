using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lynx
{
    public class LynxMenuPreprocess : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (LynxBuildSettings.FindObjectsOfTypeAll<LynxMenuMng>().Count == 0)
                Debug.LogWarning("Lynx Menu is missing. Add it to your first scene to be able to quit your application.");
        }
    }
}