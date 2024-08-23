#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif

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
        [SerializeField] private TextMeshProUGUI m_appVersionText;

        //PRIVATE
#if LYNX
        private string applicationName;
        private Texture2D iconTexture;
#endif


        private void Start()
        {
            m_appVersionText.text = "ver : " + Application.version;
#if LYNX
        //UpdateAppInfo();
        //AndroidComMng.Instance().mAndroidSystemComPlugInInitSucceedEvent.AddListener(DisplayAppInfo);
#endif
        }

        private void OnEnable()
        {
#if LYNX
        //UpdateAppInfo();
#endif
        }

        public void UpdateAppInfo()
        {
#if LYNX
            Debug.Log("----------- DisplayAppInfo()");
            applicationName = AndroidComMng.GetAppName();
            iconTexture = AndroidComMng.GetAppIconTexture();

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