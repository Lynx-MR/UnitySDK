using UnityEngine;

namespace Lynx
{
    public class ScreenshotFlash : MonoBehaviour
    {
        [SerializeField]
        Light dirLight;

        [SerializeField]
        Transform CameraTransform;

        float period = 0.2f;
        float timer = 0.0f;

        float maxIntensity = 15.0f;
        bool isFlashing = false;

        // Start is called before the first frame update
        void Start()
        {
            if (CameraTransform == null) CameraTransform = Camera.main.transform;

            timer = 0.0f;
            isFlashing = false;
            dirLight.intensity = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isFlashing) return;

            if (CameraTransform == null)
            {
                Debug.LogError("No avalaible camera in ScreenShotFlash");
                return;
            }

            transform.position = CameraTransform.position;

            dirLight.intensity = (-maxIntensity / period) * timer + maxIntensity; // linear variation. 

            timer += Time.deltaTime;

            if (timer >= period)
            {
                isFlashing = false;
                timer = 0.0f;
                dirLight.intensity = 0.0f;
            }
        }

        public void TriggerFlash()
        {
            isFlashing = true;
            timer = 0.0f;
        }
    }
}