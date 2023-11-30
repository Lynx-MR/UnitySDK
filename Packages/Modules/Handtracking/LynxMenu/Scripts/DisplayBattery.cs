#if UNITY_ANDROID && !UNITY_EDITOR
    #define LYNX
#endif

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx
{
    [RequireComponent(typeof(ActionsInUnityMainThread))]
    public class DisplayBattery : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private Image batteryValueImg;
        [SerializeField] private Sprite[] batteryValueSpriteArray;

        int batteryIsChargingFlag = 1000;

        private void Awake()
        {
#if LYNX
            AndroidComMng.OnABatteryLevelChange += onBatteryLevelChanged;
#endif
        }

        private void OnEnable()
        {          
            int batteryPercent = AndroidComMng.GetBatteryLevel();

            if (AndroidComMng.isBatteryCharging())
                batteryPercent += batteryIsChargingFlag;

            FillBatteryInfo(batteryPercent);
        }

        private void OnDestroy()
        {
#if LYNX
            AndroidComMng.OnABatteryLevelChange -= onBatteryLevelChanged;
#endif
        }

        private void onBatteryLevelChanged(int batteryLevel)
        {
            ActionsInUnityMainThread.actionsInUnityMainThread.AddJob( () => FillBatteryInfo(batteryLevel));
        }
        private void FillBatteryInfo(int batteryInfo)
        {
            int batteryLevel = 0;

            if (batteryInfo >= batteryIsChargingFlag) // then battery is Charging
            {
                batteryValueImg.sprite = batteryValueSpriteArray[0];
                batteryLevel = batteryInfo - batteryIsChargingFlag;
            }
            else
            {
                batteryLevel = batteryInfo;

                if (batteryLevel > 0 && batteryLevel < 25)
                {
                    batteryValueImg.sprite = batteryValueSpriteArray[1];
                }
                else if (batteryLevel >= 30 && batteryLevel < 75)
                {
                    batteryValueImg.sprite = batteryValueSpriteArray[2];
                }
                else if (batteryLevel >= 75 && batteryLevel < 95)
                {
                    batteryValueImg.sprite = batteryValueSpriteArray[3];
                }
                else if (batteryLevel > 95)
                {
                    batteryValueImg.sprite = batteryValueSpriteArray[4];
                }
            }

            percentText.text = batteryLevel.ToString() + "%";
        }

    }
}