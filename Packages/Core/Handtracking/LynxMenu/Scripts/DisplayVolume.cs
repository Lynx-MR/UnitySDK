#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Lynx
{
    public class DisplayVolume : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private Slider sliderUI;
        [SerializeField] private TextMeshProUGUI valueUI;
        [SerializeField] public  Image VolumeLevelImage;
        [SerializeField] private Sprite[] VolumeLevelSpriteArray;

        private Action m_mainThreadAction = null;


        private void Awake()
        {
#if LYNX
        AndroidComMng.OnAudioVolumeChange += OnAudioVolumeChanged;
#endif
        }

        private void OnEnable()
        {
            UpdateVolumeLevelDisplay();
        }

        private void OnDestroy()
        {
#if LYNX
        AndroidComMng.OnAudioVolumeChange -= OnAudioVolumeChanged;
#endif
        }

        private void Update()
        {
            lock (m_mainThreadAction)
            {
                if (m_mainThreadAction != null)
                {
                    m_mainThreadAction.Invoke();
                    m_mainThreadAction = null;
                }
            }
        }


        private void OnAudioVolumeChanged(int volume)
        {

            lock (m_mainThreadAction)
            {
                m_mainThreadAction = () => UpdateVolumeLevelDisplay(volume);
            }
        }
        private void UpdateVolumeLevelDisplay()
        {
            int currentVolume = AndroidComMng.GetAudioVolume();
            UpdateVolumeLevelDisplay(currentVolume);
        }
        private void UpdateVolumeLevelDisplay(int volume)
        {
            int MaxVolume = AndroidComMng.GetMaxAudioVolume();

            float percent = ((float)volume / (float)MaxVolume) * 100.0f;

            valueUI.text = ((int)percent).ToString() + "%";

            if (percent <= 0.0f)
            {
                VolumeLevelImage.sprite = VolumeLevelSpriteArray[0];
            }
            else if (percent > 0.0f && percent <= 25.0f)
            {
                VolumeLevelImage.sprite = VolumeLevelSpriteArray[1];
            }
            else if (percent > 25.0f && percent <= 50.0f)
            {
                VolumeLevelImage.sprite = VolumeLevelSpriteArray[2];
            }
            else if (percent > 50.0f && percent <= 75.0f)
            {
                VolumeLevelImage.sprite = VolumeLevelSpriteArray[3];
            }
            else if (percent > 75.0f)
            {
                VolumeLevelImage.sprite = VolumeLevelSpriteArray[4];
            }
        }


        // old code to manage a slider :
        public void UpdateVolumeSliderValue(int val)
        {
            valueUI.text = val + "%";
            if (sliderUI) if (sliderUI.value != ((float)val / 100.0f)) sliderUI.value = ((float)val / 100.0f);
        }
        public void UpdateVolumeFromSlider()
        {
            MiniLauncherValueTracker.Instance.volume.Set((int)(sliderUI.value * 100));
        }

    }
}