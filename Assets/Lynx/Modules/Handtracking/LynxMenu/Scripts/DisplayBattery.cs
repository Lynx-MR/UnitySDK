using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx
{
    public class DisplayBattery : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private Image batteryValueImg;
        [SerializeField] private Sprite[] batteryValueSpriteArray;

        //PRIVATE
        float timer = 0.0f;
        float batteryTimer = 2.0f;

        int batteryIsChargingFlag = 1000;


        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidComMng.Instance().mBatteryLevelChangeEvent.AddListener(onBatteryLevelChanged);       
#endif
        }

        private void Update()
        {
            // polling version : 
            /*
    #if UNITY_ANDROID && !UNITY_EDITOR
            BatteryInfoPolling();
    #endif
            */
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidComMng.Instance().mBatteryLevelChangeEvent.RemoveListener(onBatteryLevelChanged); 
#endif
        }


        private void BatteryInfoPolling()
        {
            timer += Time.deltaTime;

            if (timer >= batteryTimer)
            {
                int batteryPercent = AndroidComMng.Instance().GetBatteryLevel();
                FillBatteryInfo(batteryPercent);

                timer = 0;
            }
        }
        private void onBatteryLevelChanged(int batteryLevel)
        {
            FillBatteryInfo(batteryLevel);
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