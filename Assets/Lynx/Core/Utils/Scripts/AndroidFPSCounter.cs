using System.Collections;
using UnityEngine;

namespace Lynx
{
    public class AndroidFPSCounter : MonoBehaviour
    {
        private int FramesPerSec;
        private float frequency = 1.0f;

        private int fps;
        private float ms;
        private string strFps;
        private string strMs;

        [SerializeField]
        private TMPro.TextMeshPro FPSCounterTxt;

        [SerializeField]
        private TMPro.TextMeshPro MSCounterTxt;

        void Start()
        {
            StartCoroutine(FPS());
        }

        private IEnumerator FPS()
        {
            for (; ; )
            {
                // Capture frame-per-second
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.realtimeSinceStartup;
                yield return new WaitForSeconds(frequency);
                float timeSpan = Time.realtimeSinceStartup - lastTime;
                int frameCount = Time.frameCount - lastFrameCount;

                ms = timeSpan / frameCount * 1000.0f;
                strMs = ms.ToString("0.0") + " ms";

                fps = Mathf.RoundToInt(frameCount / timeSpan);
                strFps = string.Format("FPS: {0}", fps);
            }
        }

        void OnGUI()
        {
            FPSCounterTxt.text = strFps;
            MSCounterTxt.text = strMs;

            if (fps >= 88.0f)
            {
                FPSCounterTxt.color = Color.green;
                MSCounterTxt.color = Color.green;
            }
            else if (fps < 88.0f && fps >= 60.0f)
            {
                FPSCounterTxt.color = Color.yellow;
                MSCounterTxt.color = Color.yellow;
            }
            else
            {
                FPSCounterTxt.color = Color.red;
                MSCounterTxt.color = Color.red;
            }
        }

    }
}