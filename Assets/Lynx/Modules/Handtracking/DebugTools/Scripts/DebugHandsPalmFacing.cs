using Leap.Unity;
using UnityEngine;

namespace Lynx
{
    public class DebugHandsPalmFacing : MonoBehaviour
    {
        //PUBLIC
        public Transform cameraEye;
        public bool debugHandRight = true;
        public bool debugHandLeft = true;
        [Header("Debug 3D Elements")]
        public GameObject debugLineTemplate;
        public float palmFacingDotThreshold = 0f;

        //PRIVATE
        private GameObject debugLinePalmNormalRight;
        //private GameObject debugLinePalmToEyeRight;

        private GameObject debugLinePalmNormalLeft;
        //private GameObject debugLinePalmToEyeLeft;

        private bool showDebug = true;



        private void OnValidate()
        {
            if (cameraEye == null) cameraEye = Camera.main.gameObject.transform;
        }

        private void Start()
        {
            if (debugLineTemplate.activeSelf) debugLineTemplate.SetActive(false);
            SetupDebugLines();
        }

        private void Update()
        {
            if (showDebug) UpdateDebugLines();
        }



        //Public methods
        public void Show()
        {
            if (showDebug) return;

            if (debugHandRight) debugLinePalmNormalRight.SetActive(true);
            if (debugHandLeft) debugLinePalmNormalLeft.SetActive(true);

            showDebug = true;
        }
        public void Hide()
        {
            if (!showDebug) return;

            if (debugHandRight) debugLinePalmNormalRight.SetActive(false);
            if (debugHandLeft) debugLinePalmNormalLeft.SetActive(false);

            showDebug = false;
        }


        //Private methods


        private void SetupDebugLines()
        {
            if (debugHandRight)
            {
                debugLinePalmNormalRight = Instantiate(debugLineTemplate, debugLineTemplate.transform.parent);
                debugLinePalmNormalRight.SetActive(false);
                //debugLinePalmToEyeRight = Instantiate(debugLineTemplate, debugLineTemplate.transform.parent);
                //debugLinePalmToEyeRight.SetActive(false);
            }
            if (debugHandLeft)
            {
                debugLinePalmNormalLeft = Instantiate(debugLineTemplate, debugLineTemplate.transform.parent);
                debugLinePalmNormalLeft.SetActive(false);
                //debugLinePalmToEyeLeft = Instantiate(debugLineTemplate, debugLineTemplate.transform.parent);
                //debugLinePalmToEyeLeft.SetActive(false);
            }
        }

        private void UpdateDebugLines()
        {
            if (debugHandRight)
            {
                UpdateDebugLineHand(Hands.Provider.GetHand(Chirality.Right), ref debugLinePalmNormalRight/*, ref debugLinePalmToEyeRight*/);
            }
            if (debugHandLeft)
            {
                UpdateDebugLineHand(Hands.Provider.GetHand(Chirality.Left), ref debugLinePalmNormalLeft/*, ref debugLinePalmToEyeLeft*/);
            }
        }
        private void UpdateDebugLineHand(Leap.Hand hand, ref GameObject debugLinePalmNormal/*, ref GameObject debugLinePalmToEye*/)
        {
            //Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);
            if (hand == null)
            {
                debugLinePalmNormal.SetActive(false);
                //debugLinePalmToEye.SetActive(false);

            }
            else
            {
                debugLinePalmNormal.SetActive(true);
                //debugLinePalmToEye.SetActive(false);

                debugLinePalmNormal.transform.position = hand.PalmPosition;
                debugLinePalmNormal.transform.rotation = Quaternion.LookRotation(hand.PalmNormal);
                Vector3 palmToEye = cameraEye.transform.position - hand.PalmPosition;
                bool palmFacingEye = Vector3.Dot(hand.PalmNormal, Vector3.Normalize(palmToEye)) > palmFacingDotThreshold;
                debugLinePalmNormal.GetComponentInChildren<MeshRenderer>().material.color = palmFacingEye ? Color.green : Color.red;

                //debugLinePalmToEyeRight.transform.position = handRight.PalmPosition;
                //debugLinePalmToEyeRight.transform.LookAt(cameraEye);
            }
        }
    }
}