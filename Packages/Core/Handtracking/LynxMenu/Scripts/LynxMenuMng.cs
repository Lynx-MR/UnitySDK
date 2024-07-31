using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool lynxMenuActive = false;
        public GameObject lynxMenuMainCanvas;
        public LynxMenuGroundPosition lynxMenuGroundPosition;

        [Space]
        [SerializeField] private float LifePeriod = 20.0f;
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
        private float timerLifeCycle = 0.0f;
        private float scaleDuration = 0.2f;



        private void Start()
        {
            InitializeLynxMenu();
            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            //CheckDeactivateTimer();
        }



        private void InitializeLynxMenu()
        {
            if (lynxMenuGameObject.activeSelf) lynxMenuGameObject.SetActive(false);
            if (lynxMenuActive) lynxMenuActive = false;
        }
        private void CheckDeactivateTimer()
        {
            if (!lynxMenuActive) return;

            timerLifeCycle += Time.deltaTime;

            if (timerLifeCycle >= LifePeriod)
            {
                Debug.Log(" Lynx Menu End Of Cycle");
                DeactivateLynxMenu();
                timerLifeCycle = 0;

            }
        }

        public void SwitchLynxMenu()
        {
            Debug.Log("LynxMenuMng.SwitchLynxMenu()");
            if (!lynxMenuActive) ActivateLynxMenuWithScale();
            else DeactivateLynxMenuWithScale();
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ActivateLynxMenu()
        {
            Debug.Log("LynxMenuMng.ActivateLynxMenu()");
            if (lynxUIPositionner != null) lynxUIPositionner.ResetToTargetOffsets();

            lynxMenuGameObject.SetActive(true);
            lynxMenuMainCanvas.SetActive(true);
            if (lynxMenuGroundPosition) lynxMenuGroundPosition.lynxMenuGroundPosCanvas.SetActive(false);

            timerLifeCycle = 0.0f;
            lynxMenuActive = true;

            if (!appInfoRetrieved)
            {
                displayAppInfo.UpdateAppInfo();
                appInfoRetrieved = true;
            }

            lynxMenuOpened?.Invoke();
        }
        public void ActivateLynxMenuWithScaleOld()
        {
            Debug.Log("LynxMenuMng.ActivateLynxMenuWithScale()");
            if (lynxUIPositionner != null) lynxUIPositionner.ResetToTargetOffsets();

            lynxMenuGameObject.SetActive(true);
            timerLifeCycle = 0.0f;
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.zero, Vector3.one, scaleDuration));

            lynxMenuActive = true;

            if (!appInfoRetrieved)
            {
                displayAppInfo.UpdateAppInfo();
                appInfoRetrieved = true;
            }

            lynxMenuOpened?.Invoke();
        }

        public void ActivateLynxMenuWithScale()
        {
            Debug.Log("LynxMenuMng.ActivateLynxMenuWithScale()");
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.zero, Vector3.one, scaleDuration));
            ActivateLynxMenu();
        }

        public void DeactivateLynxMenu()
        {
            Debug.Log("LynxMenuMng.DeactivateLynxMenu()");

            if (lynxMenuGroundPosition) lynxMenuGroundPosition.DeactivateLynxMenuGroundPosition();
            lynxMenuMainCanvas.SetActive(false);
            lynxMenuGameObject.SetActive(false);

            lynxMenuActive = false;
            timerLifeCycle = 0.0f;
            lynxMenuClosed?.Invoke();
        }
        public void DeactivateLynxMenuWithScale()
        {
            Debug.Log("LynxMenuMng.DeactivateLynxMenuWithScale()");
            StartCoroutine(ScaleObject(lynxMenuGameObject, Vector3.one, Vector3.zero, scaleDuration));
            Invoke(nameof(DeactivateLynxMenu), scaleDuration);
        }

        public void recordScreen()
        {
            Debug.Log("recordScreen()");

            videoEncoderNatifMng.RecordVideo();
            screenRecordButton.SetActive(false);
            stopScreenRecordButton.SetActive(true);
        }
        public void stopRecordScreen()
        {
            Debug.Log("stopRecordScreen()");

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


    }
}