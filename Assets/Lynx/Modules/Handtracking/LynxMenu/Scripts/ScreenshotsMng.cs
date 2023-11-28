using TMPro;
using UnityEngine;

namespace Lynx
{
    public class ScreenshotsMng : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private GameObject timer;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private AudioMng audioMng;
        [SerializeField] private ScreenshotFlash launchFlash;

        //PUBLIC
        public ScreenshotAndVideoUtilities screenshotAndVideoUtilities;

        //PRIVATE
        private bool screenshotProcessActive = false;
        private float timerScreenshot = 6.0f;
        private float timerBip = 0.0f;

        private void Update()
        {
            UpdateScreenshotProcess();
        }

        public void StartScreenshotProcess()
        {
            if (screenshotProcessActive) return;

            screenshotProcessActive = true;
            timer.SetActive(true);
        }

        private void UpdateScreenshotProcess()
        {
            if (screenshotProcessActive == false) return;

            timerScreenshot -= Time.deltaTime;
            timerBip -= Time.deltaTime;

            int time = (int)timerScreenshot;

            if (timerScreenshot < 1)
            {
                screenshotProcessActive = false;
                timerScreenshot = 6.0f;
                timerBip = 0.0f;
                timer.SetActive(false);

                doScreenshot();

                Debug.Log("take shot c'est ici");

                return;
            }
            else
            {
                timerText.text = time.ToString() + "s";
            }

            if (timerBip <= 0)
            {
                audioMng.Play_Bip();
                timerBip = 1.0f;
            }
        }

        private void doScreenshot()
        {
            screenshotAndVideoUtilities.TakeShot();
            audioMng.Play_Photo_Diaph();
            launchFlash.TriggerFlash();
        }


        public void TakeDirectScreenShot()
        {
            doScreenshot();
        }
    }
}