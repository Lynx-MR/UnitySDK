using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lynx
{
    [RequireComponent(typeof(LynxMenuMng))]
    public class LynxMenuGroundPosition : MonoBehaviour
    {

        //INSPECTOR
        public GameObject lynxMenuGroundPosCanvas;
        public GameObject windowGroundPosition;
        public GameObject windowGroundPositionSetup;

        public LynxGuardian lynxGuardian;
        public LynxMenuMng lynxMenuMng;

        public GameObject floorIndicatorPrefab;


        //PUBLIC

        //PRIVATE
        private GameObject floorIndicatorInstance = null;



        private void Awake()
        {
            if (!lynxMenuMng) lynxMenuMng = GetComponent<LynxMenuMng>();
            if (!lynxGuardian) lynxGuardian = GetComponent<LynxGuardian>();
        }



        public void DisplayFloorIndicator()
        {
            if (!floorIndicatorInstance)
            {
                Vector3 relativePos = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
                Quaternion cameraYRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);

                floorIndicatorInstance = Instantiate(floorIndicatorPrefab, relativePos, cameraYRotation);
            }
        }
        public void RemoveFloorIndicator()
        {
            if (floorIndicatorInstance)
            {
                Destroy(floorIndicatorInstance);
            }
        }


        public void ActivateLynxMenuGroundPosition()
        {
            lynxMenuMng.lynxMenuMainCanvas.SetActive(false);
            lynxMenuGroundPosCanvas.SetActive(true);
            windowGroundPosition.SetActive(true);
            windowGroundPositionSetup.SetActive(false);
            DisplayFloorIndicator();
        }
        public void DeactivateLynxMenuGroundPosition()
        {
            lynxGuardian.ConfirmFloorPosition();
            RemoveFloorIndicator();
            lynxMenuGroundPosCanvas.SetActive(false);
            windowGroundPosition.SetActive(false);
            windowGroundPositionSetup.SetActive(false);
            lynxMenuMng.lynxMenuMainCanvas.SetActive(true);
        }


        //Window1
        public void ChangeGroundPosition()
        {
            Debug.Log("Change ground position");
            windowGroundPosition.SetActive(false);
            windowGroundPositionSetup.SetActive(true);

            lynxGuardian.SetupFloorPosition();
        }

        public void Return()
        {
            DeactivateLynxMenuGroundPosition();
            //RemoveFloorIndicator();
        }

        //Window2
        public void ConfirmGroundPosition()
        {
            lynxGuardian.ConfirmFloorPosition();
            lynxGuardian.SaveCameraHeight();
            lynxGuardian.ApplyCameraHeight();
            DeactivateLynxMenuGroundPosition();
        }

        public void Cancel()
        {
            DeactivateLynxMenuGroundPosition();
            //lynxGuardian.ConfirmFloorPosition();
            //RemoveFloorIndicator();
        }



    }
}