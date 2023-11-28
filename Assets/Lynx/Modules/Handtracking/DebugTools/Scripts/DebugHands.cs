using System.Text;
using UnityEngine;
using Leap.Unity;
using TMPro;

namespace Lynx
{
    public class DebugHands : MonoBehaviour
    {
        //PUBLIC
        public Transform cameraEye;
        [Header("Debug displays")]
        public GameObject debugCanvasTemplate;
        public GameObject anchorHandRight;
        public GameObject anchorHandLeft;
        [Header("Debug 3D Elements")]
        public GameObject debugCubeTemplate;
        public Color greenTrue;
        public Color redFalse;


        //PRIVATE
        private GameObject debugCanvasHandRight;
        private GameObject debugCanvasHandLeft;

        private TextMeshProUGUI debugTMPHandRight;
        private TextMeshProUGUI debugTMPHandLeft;

        //debug cubes
        private GameObject debugCubeRightIndex;
        private GameObject debugCubeRightMiddle;
        private GameObject debugCubeRightRing;
        private GameObject debugCubeRightPinky;

        private LeapProvider leapProvider = null;


        private void OnValidate()
        {
            if (cameraEye == null) cameraEye = Camera.main?.gameObject.transform;
            if (!leapProvider) leapProvider = FindObjectOfType<LeapProvider>();
        }

        private void Start()
        {
            debugCanvasTemplate.SetActive(false);
            InstantiateDebugCanvases();
            SetupHandDebugCubes();
        }

        private void Update()
        {
            UpdateFingersExtendedDebugCubes();
            UpdateDebugCanvases();
        }


        //Debug text methods
        private void InstantiateDebugCanvases()
        {
            debugCanvasHandRight = Instantiate(debugCanvasTemplate, anchorHandRight.transform);
            debugCanvasHandRight.SetActive(true);
            debugCanvasHandRight.transform.position = Vector3.zero;
            debugTMPHandRight = debugCanvasHandRight.GetComponentInChildren<TextMeshProUGUI>();

            debugCanvasHandLeft = Instantiate(debugCanvasTemplate, anchorHandLeft.transform);
            debugCanvasHandLeft.SetActive(true);
            debugCanvasHandLeft.transform.position = Vector3.zero;
            debugTMPHandLeft = debugCanvasHandLeft.GetComponentInChildren<TextMeshProUGUI>();
        }
        private void UpdateDebugCanvases()
        {
            Vector3 eyeForward = cameraEye.TransformDirection(Vector3.forward); ;

            //update pos
            Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);

            if (handRight == null) debugCanvasHandRight.SetActive(false);
            else
            {
                if (debugCanvasHandRight.activeSelf == false) debugCanvasHandRight.SetActive(true);
                Vector3 palmPos = handRight.PalmPosition;
                anchorHandRight.transform.position = palmPos + Vector3.up * 0.15f;
                anchorHandRight.transform.rotation = Quaternion.LookRotation(eyeForward);
            }
            Leap.Hand handLeft = Hands.Provider.GetHand(Chirality.Left);
            if (handLeft == null) debugCanvasHandLeft.SetActive(false);
            else
            {
                if (debugCanvasHandLeft.activeSelf == false) debugCanvasHandLeft.SetActive(true);
                Vector3 palmPos = handLeft.PalmPosition;
                anchorHandLeft.transform.position = palmPos + Vector3.up * 0.15f;
                anchorHandLeft.transform.rotation = Quaternion.LookRotation(eyeForward);
            }

            //update text
            debugTMPHandRight.text = DebugTextHandData(true);
            debugTMPHandLeft.text = DebugTextHandData(false);
        }
        private string DebugTextHandData(bool right)
        {
            Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);
            Leap.Hand handLeft = Hands.Provider.GetHand(Chirality.Left);

            // old lign former lign :
            //QuickMenuMng quickMenuMng = FindObjectOfType<QuickMenuMng>();

            // replace quickMenuMng by LynxMenuMng to fix compilation error :  
            LynxMenuMng quickMenuMng = FindObjectOfType<LynxMenuMng>();

            StringBuilder stringBuilder = new StringBuilder();

            if (right) stringBuilder.Append($"pinchRight : ").Append(handRight.IsPinching()).Append("\n");
            else stringBuilder.Append($"pinchLeft : ").Append(handLeft.IsPinching()).Append("\n");
            stringBuilder.Append("\n");

            if (right) stringBuilder.Append($"right fist strength : ").Append(handRight.GetFistStrength()).Append("\n");
            else stringBuilder.Append($"left fist strength : ").Append(handLeft.GetFistStrength()).Append("\n");
            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }

        //Debug cubes methods
        private void SetupHandDebugCubes()
        {
            if (debugCubeRightIndex != null) Destroy(debugCubeRightIndex);
            if (debugCubeRightMiddle != null) Destroy(debugCubeRightMiddle);
            if (debugCubeRightRing != null) Destroy(debugCubeRightRing);
            if (debugCubeRightPinky != null) Destroy(debugCubeRightPinky);

            debugCubeRightIndex = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
            debugCubeRightIndex.SetActive(true);
            debugCubeRightMiddle = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
            debugCubeRightMiddle.SetActive(true);
            debugCubeRightRing = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
            debugCubeRightRing.SetActive(true);
            debugCubeRightPinky = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
            debugCubeRightPinky.SetActive(true);

        }
        private void UpdateFingersExtendedDebugCubes()
        {
            //Leap.Hand handRight = handModelRight.GetLeapHand();
            Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);

            if (handRight == null)
            {
                debugCubeRightIndex.SetActive(false);
                debugCubeRightMiddle.SetActive(false);
                debugCubeRightRing.SetActive(false);
                debugCubeRightPinky.SetActive(false);
            }
            else
            {
                debugCubeRightIndex.SetActive(true);
                debugCubeRightMiddle.SetActive(true);
                debugCubeRightRing.SetActive(true);
                debugCubeRightPinky.SetActive(true);

                Vector3 indexTipPos = handRight.GetIndex().TipPosition;
                bool indexIsExtendedLeap = handRight.GetIndex().IsExtended;
                debugCubeRightIndex.transform.position = indexTipPos;
                debugCubeRightIndex.GetComponent<MeshRenderer>().material.color = indexIsExtendedLeap ? greenTrue : redFalse;

                Vector3 middleTipPos = handRight.GetMiddle().TipPosition;
                bool middleIsExtendedLeap = handRight.GetMiddle().IsExtended;
                debugCubeRightMiddle.transform.position = middleTipPos;
                debugCubeRightMiddle.GetComponent<MeshRenderer>().material.color = middleIsExtendedLeap ? greenTrue : redFalse;

                Vector3 ringTipPos = handRight.GetRing().TipPosition;
                bool ringIsExtendedLeap = handRight.GetRing().IsExtended;
                debugCubeRightRing.transform.position = ringTipPos;
                debugCubeRightRing.GetComponent<MeshRenderer>().material.color = ringIsExtendedLeap ? greenTrue : redFalse;

                Vector3 pinkyTipPos = handRight.GetPinky().TipPosition;
                bool pinkyIsExtendedLeap = handRight.GetPinky().IsExtended;
                debugCubeRightPinky.transform.position = pinkyTipPos;
                debugCubeRightPinky.GetComponent<MeshRenderer>().material.color = pinkyIsExtendedLeap ? greenTrue : redFalse;

            }
        }

    }
}