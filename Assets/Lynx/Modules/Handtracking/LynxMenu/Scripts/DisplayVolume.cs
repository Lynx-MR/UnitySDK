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
        [SerializeField] public Image VolumeLevelImage;
        [SerializeField] private Sprite[] VolumeLevelSpriteArray;


        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidComMng.Instance().mAudioVolumeChangeEvent.AddListener(OnAudioVolumeChanged);       
#endif
        }

        private void Start()
        {
            // old code to manage a slider :
            //ValueTracker.Instance.volume.ValueChanged += UpdateVolumeSliderValue;
            //UpdateVolumeSliderValue(ValueTracker.Instance.volume.Get());

#if UNITY_ANDROID && !UNITY_EDITOR
        Invoke(nameof(UpdateVolumeLevelDisplay), 1);
        //UpdateVolumeLevelDisplay();
#endif
        }

        private void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //UpdateVolumeLevelDisplay();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidComMng.Instance().mAudioVolumeChangeEvent.RemoveListener(OnAudioVolumeChanged); 
#endif
        }


        private void OnAudioVolumeChanged(int volume)
        {
            UpdateVolumeLevelDisplay(volume);
        }
        private void UpdateVolumeLevelDisplay()
        {
            int currentVolume = AndroidComMng.Instance().GetAudioVolume();
            UpdateVolumeLevelDisplay(currentVolume);
        }
        private void UpdateVolumeLevelDisplay(int volume)
        {
            int MaxVolume = AndroidComMng.Instance().GetMaxAudioVolume();

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