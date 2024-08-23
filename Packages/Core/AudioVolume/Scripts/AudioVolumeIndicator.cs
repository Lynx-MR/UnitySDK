#if UNITY_ANDROID && !UNITY_EDITOR
#define LYNX
#endif

using System.Collections;
using UnityEngine;
using TMPro;
using System;

namespace Lynx
{
    public class AudioVolumeIndicator : MonoBehaviour
    {
        //INSPECTOR
        public GameObject volumeDisplay;
        public TextMeshProUGUI volumeText;
        public MeshRenderer volumeBar;
        [Space]
        public Texture volumeOnIco;
        public Texture volumeOffIco;
        [Space]
        public float volumeDisplayTime = 2f;

        [SerializeField] private AudioMng audioMng;

        //PRIVATE
        private Action m_mainThreadAction = null;



        private void Awake()
        {
#if LYNX
        AndroidComMng.OnAudioVolumeChange += OnAudioVolumeChanged;
#endif
        }

        private void Update()
        {
            if (m_mainThreadAction != null)
            {
                m_mainThreadAction.Invoke();
                m_mainThreadAction = null;
            }

            Vector3 nPos = Camera.main.transform.rotation * Vector3.forward * 0.4f;
            this.transform.position = Camera.main.transform.position + nPos;
            this.transform.rotation = Quaternion.LookRotation(nPos);
        }

        private void Start()
        {
            if (volumeDisplay.activeSelf) volumeDisplay.SetActive(false);
        }

        private void OnDestroy()
        {
#if LYNX
        AndroidComMng.OnAudioVolumeChange -= OnAudioVolumeChanged;
#endif
        }

        private void OnAudioVolumeChanged(int volume)
        {
            m_mainThreadAction += () =>
            {
                UpdateVolumeBars(volume);
                UpdateVolumeText(volume);
                PlayTestSound();
                DisplayVolumeLevel();
            };
        }

        private void UpdateVolumeBars(int volume)
        {

            volumeBar.material.SetFloat("_volume", (float)volume / 15f);
            if (volume == 0)
                volumeBar.material.SetTexture("_MainTex", volumeOffIco);
            else
                volumeBar.material.SetTexture("_MainTex", volumeOnIco);

        }
        private void UpdateVolumeText(int volume)
        {
            int volumePercentRounded = Mathf.RoundToInt((volume / 15f) * 100);
            volumeText.text = $"{volumePercentRounded}%";
        }


        private void DisplayVolumeLevel()
        {
            StopCoroutine(DisplayVolumeLevelWithTimer());
            StartCoroutine(DisplayVolumeLevelWithTimer());
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