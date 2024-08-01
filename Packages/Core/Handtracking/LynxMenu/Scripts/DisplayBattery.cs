#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif

using System;
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

        private int batteryIsChargingFlag = 1000;

        private Action m_mainThreadAction = null;

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

        private void Update()
        {
            if (m_mainThreadAction != null)
            {
                m_mainThreadAction.Invoke();
                m_mainThreadAction = null;
            }
        }

        private void OnDestroy()
        {
#if LYNX
            AndroidComMng.OnABatteryLevelChange -= onBatteryLevelChanged;
#endif
        }

        private void onBatteryLevelChanged(int batteryLevel)
        {
            m_mainThreadAction = () => FillBatteryInfo(batteryLevel);
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