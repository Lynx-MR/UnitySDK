#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif


using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx
{
    public class SystemInfoBar : MonoBehaviour
    {
        [Tooltip("Delay between each update for battery and time (seconds)")]
        [SerializeField] public float updateDelay = 10.0f;

        [SerializeField] private TextMeshProUGUI m_clockText;

        [Header("Battery")]
        [SerializeField] private TextMeshProUGUI m_batteryText;
        [SerializeField] private Image m_batteryIcon;
        [SerializeField] private Sprite[] m_batteryValueSpriteArray;

        [Header("Volume")]
        [SerializeField] private TextMeshProUGUI m_volumeText;
        [SerializeField] private Image m_volumeIcon;
        [SerializeField] private Sprite[] m_volumeSpriteArray;


        private Action m_mainThreadAction = null;

        #region UNITY API
        private void OnEnable()
        {
            StartCoroutine(UpdateCoroutine());
            UpdateVolumeLevelDisplay();
            AndroidComMng.OnAudioVolumeChange += OnAudioVolumeChanged;
        }

        private void OnDisable()
        {
            StopCoroutine(UpdateCoroutine());
#if LYNX
            AndroidComMng.OnAudioVolumeChange -= OnAudioVolumeChanged;
#endif
        }

        private void OnDestroy()
        {
            StopCoroutine(UpdateCoroutine());
#if LYNX
            AndroidComMng.OnAudioVolumeChange -= OnAudioVolumeChanged;
#endif
        }

        private void Update()
        {
            if (m_mainThreadAction != null)
            {
                m_mainThreadAction.Invoke();
                m_mainThreadAction = null;
            }
        }

        #endregion

        #region METHODS
        IEnumerator UpdateCoroutine()
        {
            while(this.enabled)
            {
                yield return new WaitForSecondsRealtime(updateDelay);
                m_clockText.text = DateTime.Now.ToString("HH:mm");
                UpdateBatteryUI();
            }
        }

        void UpdateBatteryUI()
        {
            int batteryLevel = (int)(SystemInfo.batteryLevel * 100);

            if (SystemInfo.batteryStatus == BatteryStatus.Charging)
            {
                m_batteryIcon.sprite = m_batteryValueSpriteArray[0];
            }
            else
            {
                if (batteryLevel > 0 && batteryLevel < 25)
                    m_batteryIcon.sprite = m_batteryValueSpriteArray[1];
                
                else if (batteryLevel >= 30 && batteryLevel < 75)
                    m_batteryIcon.sprite = m_batteryValueSpriteArray[2];
                
                else if (batteryLevel >= 75 && batteryLevel < 95)
                    m_batteryIcon.sprite = m_batteryValueSpriteArray[3];
                
                else if (batteryLevel > 95)
                    m_batteryIcon.sprite = m_batteryValueSpriteArray[4];
            }

            m_batteryText.text = $"{batteryLevel}%";
        }

        private void OnAudioVolumeChanged(int volume)
        {
            m_mainThreadAction = () => UpdateVolumeLevelDisplay(volume);
        }

        private void UpdateVolumeLevelDisplay()
        {
#if LYNX
            int currentVolume = AndroidComMng.GetAudioVolume();
            UpdateVolumeLevelDisplay(currentVolume);
#endif
        }
        private void UpdateVolumeLevelDisplay(int volume)
        {
            int MaxVolume = AndroidComMng.GetMaxAudioVolume();

            float percent = ((float)volume / (float)MaxVolume) * 100.0f;

            m_volumeText.text = ((int)percent).ToString() + "%";

            if (percent <= 0.0f)
                m_volumeIcon.sprite = m_volumeSpriteArray[0];

            else if (percent > 0.0f && percent <= 25.0f)
                m_volumeIcon.sprite = m_volumeSpriteArray[1];

            else if (percent > 25.0f && percent <= 50.0f)
                m_volumeIcon.sprite = m_volumeSpriteArray[2];

            else if (percent > 50.0f && percent <= 75.0f)
                m_volumeIcon.sprite = m_volumeSpriteArray[3];

            else if (percent > 75.0f)
                m_volumeIcon.sprite = m_volumeSpriteArray[4];
        }

#endregion
    }
}