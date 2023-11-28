using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lynx
{
    public class AudioVolumeIndicator : MonoBehaviour
    {
        //INSPECTOR
        public GameObject volumeDisplay;
        public Image[] volumeBars;
        public TextMeshProUGUI volumeText;
        [Space]
        public Color colorVolumOn;
        public Color colorVolumOff;
        [Space]
        public float volumeDisplayTime = 2f;

        [SerializeField] private AudioMng audioMng;

        //PRIVATE
        private float volumeDisplayTimer;



        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidComMng.Instance().mAudioVolumeChangeEvent.AddListener(OnAudioVolumeChanged);       
#endif
        }

        private void Start()
        {
            if (volumeDisplay.activeSelf) volumeDisplay.SetActive(false);
        }

        private void OnAudioVolumeChanged(int volume)
        {
            UpdateVolumeBars(volume);
            UpdateVolumeText(volume);
            PlayTestSound();
            DisplayVolumeLevel();
        }

        private void UpdateVolumeBars(int volume)
        {
            for (int i = 0; i < 15; i++)
            {
                if (i + 1 <= volume) volumeBars[i].color = colorVolumOn;
                else volumeBars[i].color = colorVolumOff;
            }
        }
        private void UpdateVolumeText(int volume)
        {
            int volumePercentRounded = Mathf.RoundToInt((volume / 15f) * 100);
            volumeText.text = $"{volumePercentRounded}%";
        }


        private void DisplayVolumeLevel()
        {
            StopCoroutine(nameof(DisplayVolumeLevelWithTimer));
            StartCoroutine(nameof(DisplayVolumeLevelWithTimer));
        }

        private void PlayTestSound()
        {
            audioMng?.stop();
            audioMng?.playVolumeTest();
        }

        IEnumerator DisplayVolumeLevelWithTimer()
        {
            if (!volumeDisplay.activeSelf) volumeDisplay.SetActive(true);
            yield return new WaitForSecondsRealtime(volumeDisplayTime);
            volumeDisplay.SetActive(false);
        }
    }
}