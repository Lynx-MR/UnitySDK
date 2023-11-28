using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx
{
    public class DisplayAppInfo : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private TextMeshProUGUI appName;
        [SerializeField] private RawImage appIconImage;
        [SerializeField] private Image appIconImageBackground;

        //PRIVATE
        private string packageName;
        private string applicationName;
        private Texture2D iconTexture;


        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //UpdateAppInfo();
        //AndroidComMng.Instance().mAndroidSystemComPlugInInitSucceedEvent.AddListener(DisplayAppInfo);
#endif
        }

        private void OnEnable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //UpdateAppInfo();
#endif
        }

        public void UpdateAppInfo()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            return;
#else
            Debug.Log("----------- DisplayAppInfo()");
            applicationName = AndroidComMng.Instance().GetAppName();
            iconTexture = AndroidComMng.Instance().GetAppIconTexture();

            if (applicationName != null) appName.text = applicationName;
            else appName.text = "AppName";

            if (iconTexture != null)
            {
                appIconImage.texture = iconTexture;
                appIconImage.color = Color.white;
                appIconImageBackground.color = Color.black;
            }
#endif
        }
    }
}