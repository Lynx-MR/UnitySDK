using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Lynx
{
    public class LynxMenuMng : MonoBehaviour
    {
        //INSPECTOR
        public GameObject lynxMenuGameObject;
        public UI.LynxUIPositioner lynxUIPositionner;
        public GameObject lynxMenuMainCanvas;
        public LynxMenuGroundPosition lynxMenuGroundPosition;

        [Space]
        [SerializeField] private DisplayAppInfo displayAppInfo;

        [Space]
        [SerializeField] private VideoEncoderNatifMng videoEncoderNatifMng;
        [SerializeField] private GameObject screenRecordButton;
        [SerializeField] private GameObject stopScreenRecordButton;


        //PUBLIC
        [HideInInspector] public UnityEvent lynxMenuOpened = new UnityEvent();
        [HideInInspector] public UnityEvent lynxMenuClosed = new UnityEvent();


        //PRIVATE
        private bool appInfoRetrieved = false;
        private float scaleDuration = 0.2f;

        private void Start()
        {
            lynxMenuGameObject.SetActive(false);
            DontDestroyOnLoad(this.gameObject);
        }

        public void SwitchLynxMenu()
        {
            if (!lynxMenuGameObject.activeSelf)
                ActivateLynxMenuWithScale();
            else
                DeactivateLynxMenuWithScale();

            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ActivateLynxMenu()
        {
            if (lynxUIPositionner != null)
                lynxUIPositionner.ResetToTargetOffsets();

            lynxMenuGameObject.SetActive(true);
            lynxMenuMainCanvas.SetActive(true);
            if (lynxMenuGroundPosition)
                lynxMenuGroundPosition.lynxMenuGroundPosCanvas.SetActive(false);

            if (!appInfoRetrieved)
            {
                displayAppInfo.UpdateAppInfo();
                appInfoRetrieved = true;
            }

            lynxMenuOpened?.Invoke();
        }
        public void ActivateLynxMenuWithScaleOld()
        {
            if (lynxUIPositionner != null) lynxUIPositionner.ResetToTargetOffsets();

            lynxMenuGameObject.SetActive(true);
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.zero, Vector3.one, scaleDuration));

            if (!appInfoRetrieved)
            {
                displayAppInfo.UpdateAppInfo();
                appInfoRetrieved = true;
            }

            lynxMenuOpened?.Invoke();
        }

        public void ActivateLynxMenuWithScale()
        {
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.zero, Vector3.one, scaleDuration));
            ActivateLynxMenu();
        }

        public void DeactivateLynxMenu()
        {
            if (lynxMenuGroundPosition) lynxMenuGroundPosition.DeactivateLynxMenuGroundPosition();
            lynxMenuMainCanvas.SetActive(false);
            lynxMenuGameObject.SetActive(false);

            lynxMenuClosed?.Invoke();
        }
        public void DeactivateLynxMenuWithScale()
        {
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.one, Vector3.zero, scaleDuration));
            Invoke(nameof(DeactivateLynxMenu), scaleDuration);
        }

        public void recordScreen()
        {
            videoEncoderNatifMng.RecordVideo();
            screenRecordButton.SetActive(false);
            stopScreenRecordButton.SetActive(true);
        }

        public void stopRecordScreen()
        {
            videoEncoderNatifMng.StopRecord();
            screenRecordButton.SetActive(true);
            stopScreenRecordButton.SetActive(false);
        }

        private IEnumerator ScaleObject(GameObject objectToScale, Vector3 startScale, Vector3 endScale, float scaleDuration)
        {
            float startTime = Time.time;
            float elapsedTime = 0.0f;

            while (elapsedTime < scaleDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / scaleDuration);
                objectToScale.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                elapsedTime = Time.time - startTime;
                yield return null;
            }
            objectToScale.transform.localScale = endScale;
        }

        public static void Quit()
        {
            Application.Quit();
        }
    }
}